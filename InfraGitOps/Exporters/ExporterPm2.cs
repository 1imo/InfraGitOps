using InfraGitOps.Interfaces;
using InfraGitOps.Models;
using System.Diagnostics;
using System.Text.Json;

namespace InfraGitOps.Exporters;

public class ExporterPm2 : IExporter
{
    public string ComponentName => "pm2";

    public async Task<object> ExportAsync()
    {
        var manifest = new Pm2Manifest
        {
            Version = 1,
            Apps = new List<Pm2App>()
        };

        try
        {
            var output = await RunCommandAsync("pm2", "jlist");
            
            if (!string.IsNullOrWhiteSpace(output))
            {
                var processes = JsonSerializer.Deserialize<JsonElement[]>(output);
                if (processes != null)
                {
                    foreach (var proc in processes)
                    {
                        var name = proc.TryGetProperty("name", out var nameVal) ? nameVal.GetString() : "unknown";
                        var script = proc.TryGetProperty("pm2_env", out var envVal) && envVal.TryGetProperty("pm_exec_path", out var scriptVal) 
                            ? scriptVal.GetString() 
                            : "unknown";
                        var instances = proc.TryGetProperty("pm2_env", out var env2) && env2.TryGetProperty("instances", out var instVal) 
                            ? instVal.GetInt32() 
                            : 1;

                        manifest.Apps.Add(new Pm2App
                        {
                            Name = name ?? "unknown",
                            Script = script ?? "unknown",
                            Instances = instances,
                            Env = new Dictionary<string, string>()
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExporterPm2] Could not read PM2 state: {ex.Message}");
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