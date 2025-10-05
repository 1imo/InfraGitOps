using InfraGitOps.Orchestrator;
using Xunit;

namespace InfraGitOps.Tests;

public class FlowRegistryTests
{
    private readonly FlowRegistry _flowRegistry;

    public FlowRegistryTests()
    {
        _flowRegistry = new FlowRegistry();
    }

    [Fact]
    public void GetDependencies_DockerHasNoDependencies()
    {
        var dependencies = _flowRegistry.GetDependencies("docker");
        Assert.Empty(dependencies);
    }

    [Fact]
    public void GetDependencies_Pm2DependsOnDocker()
    {
        var dependencies = _flowRegistry.GetDependencies("pm2");
        Assert.Single(dependencies);
        Assert.Contains("docker", dependencies);
    }

    [Fact]
    public void GetDependencies_NginxDependsOnPm2()
    {
        var dependencies = _flowRegistry.GetDependencies("nginx");
        Assert.Single(dependencies);
        Assert.Contains("pm2", dependencies);
    }

    [Fact]
    public void GetDependents_DockerHasPm2AsDependent()
    {
        var dependents = _flowRegistry.GetDependents("docker");
        Assert.Contains("pm2", dependents);
    }

    [Fact]
    public void GetAffectedComponents_DockerChangeAffectsDockerPm2Nginx()
    {
        var affected = _flowRegistry.GetAffectedComponents("docker");
        Assert.Equal(3, affected.Count);
        Assert.Contains("docker", affected);
        Assert.Contains("pm2", affected);
        Assert.Contains("nginx", affected);
    }

    [Fact]
    public void GetAffectedComponents_ReturnsInTopologicalOrder()
    {
        var affected = _flowRegistry.GetAffectedComponents("docker");
        var dockerIndex = affected.IndexOf("docker");
        var pm2Index = affected.IndexOf("pm2");
        var nginxIndex = affected.IndexOf("nginx");

        Assert.True(dockerIndex < pm2Index);
        Assert.True(pm2Index < nginxIndex);
    }

    [Fact]
    public void TopologicalSort_SortsCorrectly()
    {
        var components = new List<string> { "nginx", "docker", "pm2" };
        var sorted = _flowRegistry.TopologicalSort(components);

        var dockerIndex = sorted.IndexOf("docker");
        var pm2Index = sorted.IndexOf("pm2");
        var nginxIndex = sorted.IndexOf("nginx");

        Assert.True(dockerIndex < pm2Index);
        Assert.True(pm2Index < nginxIndex);
    }

    [Fact]
    public void GetAllComponents_ReturnsAllFiveComponents()
    {
        var components = _flowRegistry.GetAllComponents().ToList();
        Assert.Equal(5, components.Count);
        Assert.Contains("docker", components);
        Assert.Contains("pm2", components);
        Assert.Contains("nginx", components);
        Assert.Contains("ufw", components);
        Assert.Contains("images", components);
    }
}
