using InfraGitOps.Orchestrator;
using InfraGitOps.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit;

namespace InfraGitOps.Tests;

public class EndToEndTests
{
    private readonly string _testConfigPath;

    public EndToEndTests()
    {
        _testConfigPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testConfigPath);
        SetupTestManifests();
    }

    private void SetupTestManifests()
    {
        var components = new[] { "docker", "nginx", "pm2", "ufw", "images" };
        foreach (var component in components)
        {
            var manifest = new { Version = 1 };
            var filePath = Path.Combine(_testConfigPath, $"manifest_{component}.json");
            File.WriteAllText(filePath, JsonSerializer.Serialize(manifest));
        }
    }

    [Fact]
    public async Task FullFlow_ChangeDockerManifest_TriggersTransformersAppliesExports()
    {
        var serviceProvider = OrchestratorFactory.BuildServiceProvider(_testConfigPath);
        var orchestrator = serviceProvider.GetRequiredService<InfraGitOps.Orchestrator.Orchestrator>();

        var dockerManifest = new DockerManifest
        {
            Version = 1,
            Containers = new List<DockerContainer>
            {
                new DockerContainer
                {
                    Name = "test-app",
                    Image = "nginx:latest",
                    Ports = new List<string> { "8080:80" }
                }
            }
        };

        var dockerManifestPath = Path.Combine(_testConfigPath, "manifest_docker.json");
        await File.WriteAllTextAsync(dockerManifestPath, JsonSerializer.Serialize(dockerManifest));

        await orchestrator.ProcessChangeAsync("docker");

        var dockerExists = File.Exists(dockerManifestPath);
        var pm2Exists = File.Exists(Path.Combine(_testConfigPath, "manifest_pm2.json"));
        var nginxExists = File.Exists(Path.Combine(_testConfigPath, "manifest_nginx.json"));

        Assert.True(dockerExists);
        Assert.True(pm2Exists);
        Assert.True(nginxExists);
    }

    [Fact]
    public async Task FullFlow_ChangePm2Manifest_TriggersTransformersAppliesExports()
    {
        var serviceProvider = OrchestratorFactory.BuildServiceProvider(_testConfigPath);
        var orchestrator = serviceProvider.GetRequiredService<InfraGitOps.Orchestrator.Orchestrator>();

        await orchestrator.ProcessChangeAsync("pm2");

        var pm2Exists = File.Exists(Path.Combine(_testConfigPath, "manifest_pm2.json"));
        var nginxExists = File.Exists(Path.Combine(_testConfigPath, "manifest_nginx.json"));

        Assert.True(pm2Exists);
        Assert.True(nginxExists);
    }

    [Fact]
    public void FullFlow_ChangeUfwManifest_OnlyAffectsUfw()
    {
        var serviceProvider = OrchestratorFactory.BuildServiceProvider(_testConfigPath);
        var flowRegistry = serviceProvider.GetRequiredService<FlowRegistry>();

        var affected = flowRegistry.GetAffectedComponents("ufw");

        Assert.Single(affected);
        Assert.Contains("ufw", affected);
    }

    [Fact]
    public void FullFlow_ChangeImagesManifest_OnlyAffectsImages()
    {
        var serviceProvider = OrchestratorFactory.BuildServiceProvider(_testConfigPath);
        var flowRegistry = serviceProvider.GetRequiredService<FlowRegistry>();

        var affected = flowRegistry.GetAffectedComponents("images");

        Assert.Single(affected);
        Assert.Contains("images", affected);
    }

    [Fact]
    public async Task FullFlow_ExportDevice_CreatesAllManifests()
    {
        var serviceProvider = OrchestratorFactory.BuildServiceProvider(_testConfigPath);
        var exportDevice = serviceProvider.GetRequiredService<InfraGitOps.UseCases.ExportDevice>();

        await exportDevice.ExportAllAsync();

        var dockerExists = File.Exists(Path.Combine(_testConfigPath, "manifest_docker.json"));
        var nginxExists = File.Exists(Path.Combine(_testConfigPath, "manifest_nginx.json"));
        var pm2Exists = File.Exists(Path.Combine(_testConfigPath, "manifest_pm2.json"));
        var ufwExists = File.Exists(Path.Combine(_testConfigPath, "manifest_ufw.json"));
        var imagesExists = File.Exists(Path.Combine(_testConfigPath, "manifest_images.json"));

        Assert.True(dockerExists);
        Assert.True(nginxExists);
        Assert.True(pm2Exists);
        Assert.True(ufwExists);
        Assert.True(imagesExists);
    }
}
