using InfraGitOps.UseCases;
using InfraGitOps.Models;
using System.Text.Json;
using Xunit;

namespace InfraGitOps.Tests;

public class UseCaseTests
{
    private readonly string _testConfigPath;

    public UseCaseTests()
    {
        _testConfigPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testConfigPath);
    }

    [Fact]
    public async Task ImportManifest_LoadsManifestSuccessfully()
    {
        var testManifest = new { Version = 1, TestProperty = "test-value" };
        var filePath = Path.Combine(_testConfigPath, "manifest_docker.json");
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(testManifest));

        var importManifest = new ImportManifest(_testConfigPath);
        var result = await importManifest.ImportAsync("docker");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task ImportManifest_ThrowsExceptionWhenFileNotFound()
    {
        var importManifest = new ImportManifest(_testConfigPath);

        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await importManifest.ImportAsync("nonexistent")
        );
    }

    [Fact]
    public async Task ValidateManifest_PassesForValidManifest()
    {
        var validateManifest = new ValidateManifest();
        var manifest = new { Version = 1 };

        var result = await validateManifest.ValidateAsync(manifest, "docker");

        Assert.True(result);
    }

    [Fact]
    public async Task ValidateManifest_FailsForNullManifest()
    {
        var validateManifest = new ValidateManifest();

        var result = await validateManifest.ValidateAsync(null!, "docker");

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateManifest_FailsForManifestWithoutVersion()
    {
        var validateManifest = new ValidateManifest();
        var manifest = new { SomeProperty = "value" };

        var result = await validateManifest.ValidateAsync(manifest, "docker");

        Assert.False(result);
    }
}
