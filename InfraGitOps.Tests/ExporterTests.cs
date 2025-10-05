using InfraGitOps.Exporters;
using InfraGitOps.Models;
using System.Text.Json;
using Xunit;

namespace InfraGitOps.Tests;

public class ExporterTests
{
    [Fact]
    public async Task ExporterDocker_ReturnsValidManifest()
    {
        var exporter = new ExporterDocker();
        var result = await exporter.ExportAsync();

        var json = JsonSerializer.Serialize(result);
        var manifest = JsonSerializer.Deserialize<DockerManifest>(json);

        Assert.NotNull(manifest);
        Assert.Equal(1, manifest.Version);
        Assert.NotNull(manifest.Containers);
        Assert.NotEmpty(manifest.Containers);
    }

    [Fact]
    public async Task ExporterNginx_ReturnsValidManifest()
    {
        var exporter = new ExporterNginx();
        var result = await exporter.ExportAsync();

        var json = JsonSerializer.Serialize(result);
        var manifest = JsonSerializer.Deserialize<NginxManifest>(json);

        Assert.NotNull(manifest);
        Assert.Equal(1, manifest.Version);
        Assert.NotNull(manifest.Servers);
        Assert.NotEmpty(manifest.Servers);
    }

    [Fact]
    public async Task ExporterPm2_ReturnsValidManifest()
    {
        var exporter = new ExporterPm2();
        var result = await exporter.ExportAsync();

        var json = JsonSerializer.Serialize(result);
        var manifest = JsonSerializer.Deserialize<Pm2Manifest>(json);

        Assert.NotNull(manifest);
        Assert.Equal(1, manifest.Version);
        Assert.NotNull(manifest.Apps);
        Assert.NotEmpty(manifest.Apps);
    }

    [Fact]
    public async Task ExporterUfw_ReturnsValidManifest()
    {
        var exporter = new ExporterUfw();
        var result = await exporter.ExportAsync();

        var json = JsonSerializer.Serialize(result);
        var manifest = JsonSerializer.Deserialize<UfwManifest>(json);

        Assert.NotNull(manifest);
        Assert.Equal(1, manifest.Version);
        Assert.NotNull(manifest.Rules);
        Assert.NotEmpty(manifest.Rules);
    }

    [Fact]
    public async Task ExporterImages_ReturnsValidManifest()
    {
        var exporter = new ExporterImages();
        var result = await exporter.ExportAsync();

        var json = JsonSerializer.Serialize(result);
        var manifest = JsonSerializer.Deserialize<ImagesManifest>(json);

        Assert.NotNull(manifest);
        Assert.Equal(1, manifest.Version);
        Assert.NotNull(manifest.Images);
        Assert.NotEmpty(manifest.Images);
    }

    [Fact]
    public void AllExporters_HaveCorrectComponentNames()
    {
        Assert.Equal("docker", new ExporterDocker().ComponentName);
        Assert.Equal("nginx", new ExporterNginx().ComponentName);
        Assert.Equal("pm2", new ExporterPm2().ComponentName);
        Assert.Equal("ufw", new ExporterUfw().ComponentName);
        Assert.Equal("images", new ExporterImages().ComponentName);
    }
}
