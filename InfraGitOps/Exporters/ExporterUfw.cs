using InfraGitOps.Interfaces;
using InfraGitOps.Models;
using System.Diagnostics;

namespace InfraGitOps.Exporters;

public class ExporterUfw : IExporter
{
    public string ComponentName => "ufw";

    public async Task<object> ExportAsync()
    {
        var manifest = new UfwManifest
        {
            Version = 1,
            Rules = new List<UfwRule>()
        };

        try
        {
            var output = await RunCommandAsync("ufw", "status numbered");
            
            if (!string.IsNullOrWhiteSpace(output))
            {
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Contains("ALLOW") || line.Contains("DENY") || line.Contains("REJECT"))
                    {
                        var rule = ParseUfwRule(line);
                        if (rule != null)
                        {
                            manifest.Rules.Add(rule);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExporterUfw] Could not read UFW state: {ex.Message}");
        }

        return manifest;
    }

    private UfwRule? ParseUfwRule(string line)
    {
        var action = line.Contains("ALLOW") ? "allow" : 
                     line.Contains("DENY") ? "deny" : 
                     line.Contains("REJECT") ? "reject" : null;
        
        if (action == null)
            return null;

        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var port = "any";
        var protocol = "any";

        foreach (var part in parts)
        {
            if (int.TryParse(part, out _) || part.Contains("/"))
            {
                if (part.Contains("/"))
                {
                    var portProto = part.Split('/');
                    port = portProto[0];
                    protocol = portProto.Length > 1 ? portProto[1] : "any";
                }
                else
                {
                    port = part;
                }
            }
        }

        return new UfwRule
        {
            Name = $"{action}-{port}",
            Action = action,
            Port = port,
            Protocol = protocol
        };
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
