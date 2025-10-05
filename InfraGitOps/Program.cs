using InfraGitOps.Orchestrator;
using InfraGitOps.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace InfraGitOps;

public class Program
{
    public static async Task Main(string[] args)
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "Config");

        if (!Directory.Exists(configPath))
        {
            Directory.CreateDirectory(configPath);
            Console.WriteLine($"Created config directory: {configPath}");
        }

        var serviceProvider = OrchestratorFactory.BuildServiceProvider(configPath);
        var exportDevice = serviceProvider.GetRequiredService<ExportDevice>();
        var orchestrator = serviceProvider.GetRequiredService<Orchestrator.Orchestrator>();

        Console.WriteLine("InfraGitOps System Starting");
        Console.WriteLine("============================\n");

        Console.WriteLine("Running initial export (host → manifests)...\n");
        await exportDevice.ExportAllAsync();

        Console.WriteLine("\n--- Exported Manifests ---\n");
        await DisplayManifestContents(configPath, "docker");
        await DisplayManifestContents(configPath, "pm2");
        await DisplayManifestContents(configPath, "nginx");
        await DisplayManifestContents(configPath, "ufw");
        await DisplayManifestContents(configPath, "images");

        Console.WriteLine("\n============================");
        Console.WriteLine("Initial export complete!");
        Console.WriteLine("\nOptions:");
        Console.WriteLine("  'w' - Start FileSystemWatcher and monitoring mode");
        Console.WriteLine("  'e' - Run export again");
        Console.WriteLine("  't' - Trigger test change on docker component");
        Console.WriteLine("  'q' - Quit");
        
        while (true)
        {
            var key = Console.ReadKey(true);
            
            if (key.KeyChar == 'q' || key.KeyChar == 'Q')
            {
                break;
            }
            else if (key.KeyChar == 'w' || key.KeyChar == 'W')
            {
                Console.WriteLine("\nStarting FileSystemWatcher...");
                orchestrator.Start();
                Console.WriteLine("Now watching for manifest changes. Press 'q' to quit.");
            }
            else if (key.KeyChar == 'e' || key.KeyChar == 'E')
            {
                Console.WriteLine("\nRunning export...");
                await exportDevice.ExportAllAsync();
                Console.WriteLine("Export completed!");
            }
            else if (key.KeyChar == 't' || key.KeyChar == 'T')
            {
                Console.WriteLine("\nTriggering test change on docker component...");
                await orchestrator.ProcessChangeAsync("docker");
            }
        }

        orchestrator.Stop();
        Console.WriteLine("\nInfraGitOps System Stopped");
    }

    private static async Task DisplayManifestContents(string configPath, string component)
    {
        var filePath = Path.Combine(configPath, $"manifest_{component}.json");
        if (File.Exists(filePath))
        {
            var content = await File.ReadAllTextAsync(filePath);
            Console.WriteLine($"📄 manifest_{component}.json:");
            Console.WriteLine(content);
            Console.WriteLine();
        }
    }
}
