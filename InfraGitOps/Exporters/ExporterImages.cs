using InfraGitOps.Interfaces;
using InfraGitOps.Models;
using System.Diagnostics;

namespace InfraGitOps.Exporters;

public class ExporterImages : IExporter
{
    public string ComponentName => "images";

    public async Task<object> ExportAsync()
    {
        var manifest = new ImagesManifest
        {
            Version = 1,
            Images = new List<ImageDefinition>()
        };

        try
        {
            var output = await RunCommandAsync("docker", "images --format \"{{.Repository}}|{{.Tag}}|{{.ID}}\"");
            
            if (!string.IsNullOrWhiteSpace(output))
            {
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 2 && parts[0] != "<none>")
                    {
                        manifest.Images.Add(new ImageDefinition
                        {
                            Name = parts[0],
                            Repository = parts[0],
                            Tag = parts[1]
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExporterImages] Could not read Docker images: {ex.Message}");
        }

        return manifest;
    }

    private async Task<string> RunCommandAsync(string command, string arguments)
    {
        try
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
        catch
        {
            return string.Empty;
        }
    }
}
