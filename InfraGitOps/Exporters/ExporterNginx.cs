using InfraGitOps.Interfaces;
using InfraGitOps.Models;
using System.Diagnostics;

namespace InfraGitOps.Exporters;

public class ExporterNginx : IExporter
{
    public string ComponentName => "nginx";

    public async Task<object> ExportAsync()
    {
        var manifest = new NginxManifest
        {
            Version = 1,
            Servers = new List<NginxServer>()
        };

        try
        {
            var configPaths = new[]
            {
                "/etc/nginx/sites-enabled",
                "/etc/nginx/conf.d",
                "/usr/local/etc/nginx/servers"
            };

            foreach (var configPath in configPaths)
            {
                if (Directory.Exists(configPath))
                {
                    var files = Directory.GetFiles(configPath, "*.conf", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var content = await File.ReadAllTextAsync(file);
                        var server = ParseNginxConfig(content, Path.GetFileName(file));
                        if (server != null)
                        {
                            manifest.Servers.Add(server);
                        }
                    }
                    break;
                }
            }

            if (manifest.Servers.Count == 0)
            {
                var status = await RunCommandAsync("systemctl", "is-active nginx");
                if (status.Trim() == "active")
                {
                    manifest.Servers.Add(new NginxServer
                    {
                        ServerName = "nginx-running",
                        Port = 80,
                        ProxyPass = "nginx is active but config not parsed"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExporterNginx] Could not read Nginx state: {ex.Message}");
        }

        return manifest;
    }

    private NginxServer? ParseNginxConfig(string content, string fileName)
    {
        var lines = content.Split('\n');
        string? serverName = null;
        int port = 80;
        string? proxyPass = null;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("server_name"))
            {
                var parts = trimmed.Split(new[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                    serverName = parts[1];
            }
            else if (trimmed.StartsWith("listen"))
            {
                var parts = trimmed.Split(new[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1 && int.TryParse(parts[1], out var parsedPort))
                    port = parsedPort;
            }
            else if (trimmed.StartsWith("proxy_pass"))
            {
                var parts = trimmed.Split(new[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                    proxyPass = parts[1];
            }
        }

        if (serverName != null || proxyPass != null)
        {
            return new NginxServer
            {
                ServerName = serverName ?? fileName,
                Port = port,
                ProxyPass = proxyPass ?? "none"
            };
        }

        return null;
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