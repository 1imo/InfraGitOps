using InfraGitOps.Interfaces;
using InfraGitOps.Models;
using System.Text.Json;

namespace InfraGitOps.Transformers;

public class TransformerDockerToPm2 : ITransformer
{
    public string SourceComponent => "docker";
    public string TargetComponent => "pm2";

    public async Task<object> TransformAsync(object sourceManifest)
    {
        Console.WriteLine($"[TransformerDockerToPm2] Transforming Docker manifest to PM2 manifest");

        var json = JsonSerializer.Serialize(sourceManifest);
        var dockerManifest = JsonSerializer.Deserialize<DockerManifest>(json);

        var pm2Manifest = new Pm2Manifest
        {
            Version = 1,
            Apps = new List<Pm2App>()
        };

        if (dockerManifest?.Containers != null)
        {
            foreach (var container in dockerManifest.Containers)
            {
                pm2Manifest.Apps.Add(new Pm2App
                {
                    Name = $"pm2-{container.Name}",
                    Script = "app.js",
                    Instances = 2,
                    Env = new Dictionary<string, string>
                    {
                        { "DOCKER_IMAGE", container.Image ?? "" },
                        { "CONTAINER_NAME", container.Name ?? "" }
                    }
                });
            }
        }

        await Task.Delay(50);
        return pm2Manifest;
    }
}
