using InfraGitOps.Interfaces;
using InfraGitOps.Models;
using System.Text.Json;

namespace InfraGitOps.Transformers;

public class TransformerPm2ToNginx : ITransformer
{
    public string SourceComponent => "pm2";
    public string TargetComponent => "nginx";

    public async Task<object> TransformAsync(object sourceManifest)
    {
        Console.WriteLine($"[TransformerPm2ToNginx] Transforming PM2 manifest to Nginx manifest");

        var json = JsonSerializer.Serialize(sourceManifest);
        var pm2Manifest = JsonSerializer.Deserialize<Pm2Manifest>(json);

        var nginxManifest = new NginxManifest
        {
            Version = 1,
            Servers = new List<NginxServer>()
        };

        if (pm2Manifest?.Apps != null)
        {
            var portCounter = 8080;
            foreach (var app in pm2Manifest.Apps)
            {
                nginxManifest.Servers.Add(new NginxServer
                {
                    ServerName = $"{app.Name}.local",
                    Port = 80,
                    ProxyPass = $"http://localhost:{portCounter}"
                });
                portCounter += 10;
            }
        }

        await Task.Delay(50);
        return nginxManifest;
    }
}
