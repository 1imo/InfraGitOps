using InfraGitOps.Interfaces;
using InfraGitOps.Models;
using System.Diagnostics;
using System.Text.Json;

namespace InfraGitOps.Exporters;

public class ExporterDocker : IExporter
{
    public string ComponentName => "docker";

    public async Task<object> ExportAsync()
    {
        var manifest = new DockerManifest
        {
            Version = 1,
            Containers = new List<DockerContainer>()
        };

        try
        {
            var output = await RunCommandAsync("docker", "ps --format \"{{.ID}}|{{.Names}}|{{.Image}}|{{.Ports}}\"");
            
            if (!string.IsNullOrWhiteSpace(output))
            {
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 4)
                    {
                        manifest.Containers.Add(new DockerContainer
                        {
                            Name = parts[1],
                            Image = parts[2],
                            Ports = string.IsNullOrWhiteSpace(parts[3]) 
                                ? new List<string>() 
                                : new List<string> { parts[3] }
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExporterDocker] Could not read Docker state: {ex.Message}");
        }

        return manifest;
    }

    private async Task<string> RunCommandAsync(string command, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
            return string.Empty;

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        return process.ExitCode == 0 ? output : string.Empty;
    }
}