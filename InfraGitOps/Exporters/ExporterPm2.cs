using InfraGitOps.Interfaces;
using InfraGitOps.Models;

namespace InfraGitOps.Exporters;

public class ExporterPm2 : IExporter
{
    public string ComponentName => "pm2";

    public async Task<object> ExportAsync()
    {
        await Task.Delay(50);
        
        return new Pm2Manifest
        {
            Version = 1,
            Apps = new List<Pm2App>
            {
                new Pm2App
                {
                    Name = "main-app",
                    Script = "index.js",
                    Instances = 2,
                    Env = new Dictionary<string, string>
                    {
                        { "NODE_ENV", "production" }
                    }
                }
            }
        };
    }
}