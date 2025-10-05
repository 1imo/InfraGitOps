using InfraGitOps.Interfaces;
using InfraGitOps.Models;

namespace InfraGitOps.Exporters;

public class ExporterDocker : IExporter
{
    public string ComponentName => "docker";

    public async Task<object> ExportAsync()
    {
        await Task.Delay(50);
        
        return new DockerManifest
        {
            Version = 1,
            Containers = new List<DockerContainer>
            {
                new DockerContainer
                {
                    Name = "app-container",
                    Image = "node:18",
                    Ports = new List<string> { "3000:3000" }
                }
            }
        };
    }
}