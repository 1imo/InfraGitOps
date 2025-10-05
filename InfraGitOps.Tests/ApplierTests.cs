using InfraGitOps.Scripts;
using InfraGitOps.Models;
using Xunit;

namespace InfraGitOps.Tests;

public class ApplierTests
{
    [Fact]
    public async Task ApplyDocker_ExecutesSuccessfully()
    {
        var applier = new ApplyDocker();
        var manifest = new DockerManifest { Version = 1 };

        await applier.ApplyAsync(manifest);

        Assert.Equal("docker", applier.ComponentName);
    }

    [Fact]
    public async Task ApplyNginx_ExecutesSuccessfully()
    {
        var applier = new ApplyNginx();
        var manifest = new NginxManifest { Version = 1 };

        await applier.ApplyAsync(manifest);

        Assert.Equal("nginx", applier.ComponentName);
    }

    [Fact]
    public async Task ApplyPm2_ExecutesSuccessfully()
    {
        var applier = new ApplyPm2();
        var manifest = new Pm2Manifest { Version = 1 };

        await applier.ApplyAsync(manifest);

        Assert.Equal("pm2", applier.ComponentName);
    }

    [Fact]
    public async Task ApplyUfw_ExecutesSuccessfully()
    {
        var applier = new ApplyUfw();
        var manifest = new UfwManifest { Version = 1 };

        await applier.ApplyAsync(manifest);

        Assert.Equal("ufw", applier.ComponentName);
    }

    [Fact]
    public async Task ApplyImages_ExecutesSuccessfully()
    {
        var applier = new ApplyImages();
        var manifest = new ImagesManifest { Version = 1 };

        await applier.ApplyAsync(manifest);

        Assert.Equal("images", applier.ComponentName);
    }

    [Fact]
    public void AllAppliers_HaveCorrectComponentNames()
    {
        Assert.Equal("docker", new ApplyDocker().ComponentName);
        Assert.Equal("nginx", new ApplyNginx().ComponentName);
        Assert.Equal("pm2", new ApplyPm2().ComponentName);
        Assert.Equal("ufw", new ApplyUfw().ComponentName);
        Assert.Equal("images", new ApplyImages().ComponentName);
    }
}
