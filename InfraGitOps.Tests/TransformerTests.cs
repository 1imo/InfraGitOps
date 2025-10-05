using InfraGitOps.Models;
using InfraGitOps.Transformers;
using System.Text.Json;
using Xunit;

namespace InfraGitOps.Tests;

public class TransformerTests
{
    [Fact]
    public async Task TransformerDockerToPm2_TransformsCorrectly()
    {
        var transformer = new TransformerDockerToPm2();
        var dockerManifest = new DockerManifest
        {
            Version = 1,
            Containers = new List<DockerContainer>
            {
                new DockerContainer
                {
                    Name = "test-container",
                    Image = "nginx:latest",
                    Ports = new List<string> { "80:80" }
                }
            }
        };

        var result = await transformer.TransformAsync(dockerManifest);
        var json = JsonSerializer.Serialize(result);
        var pm2Manifest = JsonSerializer.Deserialize<Pm2Manifest>(json);

        Assert.NotNull(pm2Manifest);
        Assert.Equal(1, pm2Manifest.Version);
        Assert.NotNull(pm2Manifest.Apps);
        Assert.Single(pm2Manifest.Apps);
        Assert.Equal("pm2-test-container", pm2Manifest.Apps[0].Name);
    }

    [Fact]
    public async Task TransformerPm2ToNginx_TransformsCorrectly()
    {
        var transformer = new TransformerPm2ToNginx();
        var pm2Manifest = new Pm2Manifest
        {
            Version = 1,
            Apps = new List<Pm2App>
            {
                new Pm2App
                {
                    Name = "api-service",
                    Script = "server.js",
                    Instances = 2
                }
            }
        };

        var result = await transformer.TransformAsync(pm2Manifest);
        var json = JsonSerializer.Serialize(result);
        var nginxManifest = JsonSerializer.Deserialize<NginxManifest>(json);

        Assert.NotNull(nginxManifest);
        Assert.Equal(1, nginxManifest.Version);
        Assert.NotNull(nginxManifest.Servers);
        Assert.Single(nginxManifest.Servers);
        Assert.Equal("api-service.local", nginxManifest.Servers[0].ServerName);
    }

    [Fact]
    public void TransformerDockerToPm2_HasCorrectComponents()
    {
        var transformer = new TransformerDockerToPm2();
        Assert.Equal("docker", transformer.SourceComponent);
        Assert.Equal("pm2", transformer.TargetComponent);
    }

    [Fact]
    public void TransformerPm2ToNginx_HasCorrectComponents()
    {
        var transformer = new TransformerPm2ToNginx();
        Assert.Equal("pm2", transformer.SourceComponent);
        Assert.Equal("nginx", transformer.TargetComponent);
    }
}
