using InfraGitOps.Interfaces;
using InfraGitOps.UseCases;

namespace InfraGitOps.Orchestrator;

public class Orchestrator
{
    private readonly FlowRegistry _flowRegistry;
    private readonly ApplyManifest _applyManifest;
    private readonly ExportDevice _exportDevice;
    private readonly IEnumerable<IWorkflowOrchestration> _workflowOrchestrations;
    private readonly FileSystemWatcher _watcher;
    private readonly string _configPath;
    private readonly SemaphoreSlim _processingLock;

    public Orchestrator(
        FlowRegistry flowRegistry,
        ApplyManifest applyManifest,
        ExportDevice exportDevice,
        IEnumerable<IWorkflowOrchestration> workflowOrchestrations,
        string configPath)
    {
        _flowRegistry = flowRegistry;
        _applyManifest = applyManifest;
        _exportDevice = exportDevice;
        _workflowOrchestrations = workflowOrchestrations;
        _configPath = configPath;
        _processingLock = new SemaphoreSlim(1, 1);

        _watcher = new FileSystemWatcher(_configPath)
        {
            Filter = "*.json",
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
        };

        _watcher.Changed += OnManifestChanged;
        _watcher.Created += OnManifestChanged;
    }

    public void Start()
    {
        Console.WriteLine($"Starting orchestrator, watching {_configPath}");
        _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        Console.WriteLine("Stopping orchestrator");
        _watcher.EnableRaisingEvents = false;
    }

    private async void OnManifestChanged(object sender, FileSystemEventArgs e)
    {
        await _processingLock.WaitAsync();
        try
        {
            var fileName = Path.GetFileNameWithoutExtension(e.Name);
            if (string.IsNullOrEmpty(fileName) || !fileName.StartsWith("manifest_"))
                return;

            var component = fileName.Replace("manifest_", "");
            Console.WriteLine($"Detected change in {component} manifest");

            await ProcessChangeAsync(component);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing manifest change: {ex.Message}");
        }
        finally
        {
            _processingLock.Release();
        }
    }

    public async Task ProcessChangeAsync(string changedComponent)
    {
        try
        {
            var affectedComponents = _flowRegistry.GetAffectedComponents(changedComponent);
            Console.WriteLine($"Affected components: {string.Join(", ", affectedComponents)}");

            foreach (var component in affectedComponents)
            {
                await _applyManifest.ApplyAsync(component);
            }

            await _exportDevice.ExportAllAsync();

            await CommitAndPushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ProcessChangeAsync: {ex.Message}");
            throw;
        }
    }

    private async Task CommitAndPushAsync()
    {
        try
        {
            Console.WriteLine("Committing and pushing changes to Git");

            await RunGitCommandAsync("add", "Config/*");
            
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            await RunGitCommandAsync("commit", $"-m \"Auto-update manifests {timestamp}\"");
            
            await RunGitCommandAsync("push", "origin main");

            Console.WriteLine("Successfully pushed changes to remote repository");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Git operation warning: {ex.Message}");
        }
    }

    private async Task RunGitCommandAsync(string command, string arguments)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "git",
            Arguments = $"{command} {arguments}",
            WorkingDirectory = Directory.GetParent(_configPath)?.FullName ?? _configPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(startInfo);
        if (process != null)
        {
            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new InvalidOperationException($"Git {command} failed: {error}");
            }
        }
    }
}