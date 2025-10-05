using InfraGitOps.Orchestrator;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit;

namespace InfraGitOps.Tests;

public class IntegrationTests
{
    private readonly string _testConfigPath;

    public IntegrationTests()
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
    public void OrchestratorFactory_BuildsServiceProviderSuccessfully()
    {
        var serviceProvider = OrchestratorFactory.BuildServiceProvider(_testConfigPath);

        Assert.NotNull(serviceProvider);
        var orchestrator = serviceProvider.GetRequiredService<InfraGitOps.Orchestrator.Orchestrator>();
        Assert.NotNull(orchestrator);
    }

    [Fact]
    public void OrchestratorFactory_RegistersAllAppliers()
    {
        var serviceProvider = OrchestratorFactory.BuildServiceProvider(_testConfigPath);
        var appliers = serviceProvider.GetServices<InfraGitOps.Interfaces.IApplier>().ToList();

        Assert.Equal(5, appliers.Count);
        Assert.Contains(appliers, a => a.ComponentName == "docker");
        Assert.Contains(appliers, a => a.ComponentName == "nginx");
        Assert.Contains(appliers, a => a.ComponentName == "pm2");
        Assert.Contains(appliers, a => a.ComponentName == "ufw");
        Assert.Contains(appliers, a => a.ComponentName == "images");
    }

    [Fact]
    public void OrchestratorFactory_RegistersAllExporters()
    {
        var serviceProvider = OrchestratorFactory.BuildServiceProvider(_testConfigPath);
        var exporters = serviceProvider.GetServices<InfraGitOps.Interfaces.IExporter>().ToList();

        Assert.Equal(5, exporters.Count);
        Assert.Contains(exporters, e => e.ComponentName == "docker");
        Assert.Contains(exporters, e => e.ComponentName == "nginx");
        Assert.Contains(exporters, e => e.ComponentName == "pm2");
        Assert.Contains(exporters, e => e.ComponentName == "ufw");
        Assert.Contains(exporters, e => e.ComponentName == "images");
    }

    [Fact]
    public void OrchestratorFactory_RegistersAllTransformers()
    {
        var serviceProvider = OrchestratorFactory.BuildServiceProvider(_testConfigPath);
        var transformers = serviceProvider.GetServices<InfraGitOps.Interfaces.ITransformer>().ToList();

        Assert.Equal(2, transformers.Count);
        Assert.Contains(transformers, t => t.SourceComponent == "docker" && t.TargetComponent == "pm2");
        Assert.Contains(transformers, t => t.SourceComponent == "pm2" && t.TargetComponent == "nginx");
    }

    [Fact]
    public void OrchestratorFactory_RegistersAllWorkflowOrchestrations()
    {
        var serviceProvider = OrchestratorFactory.BuildServiceProvider(_testConfigPath);
        var workflows = serviceProvider.GetServices<InfraGitOps.Interfaces.IWorkflowOrchestration>().ToList();

        Assert.Equal(5, workflows.Count);
        Assert.Contains(workflows, w => w.ComponentName == "docker");
        Assert.Contains(workflows, w => w.ComponentName == "nginx");
        Assert.Contains(workflows, w => w.ComponentName == "pm2");
        Assert.Contains(workflows, w => w.ComponentName == "ufw");
        Assert.Contains(workflows, w => w.ComponentName == "images");
    }

    [Fact]
    public async Task Orchestrator_ProcessesChangeSuccessfully()
    {
        var serviceProvider = OrchestratorFactory.BuildServiceProvider(_testConfigPath);
        var orchestrator = serviceProvider.GetRequiredService<InfraGitOps.Orchestrator.Orchestrator>();

        await orchestrator.ProcessChangeAsync("docker");

        var dockerManifestPath = Path.Combine(_testConfigPath, "manifest_docker.json");
        Assert.True(File.Exists(dockerManifestPath));
    }
}
