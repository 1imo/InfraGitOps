using InfraGitOps.Interfaces;
using InfraGitOps.Models;

namespace InfraGitOps.Exporters;

public class ExporterImages : IExporter
{
    public string ComponentName => "images";

    public async Task<object> ExportAsync()
    {
        await Task.Delay(50);
        
        return new ImagesManifest
        {
            Version = 1,
            Images = new List<ImageDefinition>
            {
                new ImageDefinition
                {
                    Name = "node-base",
                    Repository = "node",
                    Tag = "18-alpine"
                }
            }
        };
    }
}
