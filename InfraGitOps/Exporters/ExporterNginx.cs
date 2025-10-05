using InfraGitOps.Interfaces;
using InfraGitOps.Models;

namespace InfraGitOps.Exporters;

public class ExporterNginx : IExporter
{
    public string ComponentName => "nginx";

    public async Task<object> ExportAsync()
    {
        await Task.Delay(50);
        
        return new NginxManifest
        {
            Version = 1,
            Servers = new List<NginxServer>
            {
                new NginxServer
                {
                    ServerName = "app.local",
                    Port = 80,
                    ProxyPass = "http://localhost:3000"
                }
            }
        };
    }
}