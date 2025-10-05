using InfraGitOps.Interfaces;
using InfraGitOps.Models;

namespace InfraGitOps.Exporters;

public class ExporterUfw : IExporter
{
    public string ComponentName => "ufw";

    public async Task<object> ExportAsync()
    {
        await Task.Delay(50);
        
        return new UfwManifest
        {
            Version = 1,
            Rules = new List<UfwRule>
            {
                new UfwRule
                {
                    Name = "allow-http",
                    Action = "allow",
                    Port = "80",
                    Protocol = "tcp"
                }
            }
        };
    }
}
