using System.Text.Json;

namespace InfraGitOps.UseCases;

public class ImportManifest
{
    private readonly string _configPath;

    public ImportManifest(string configPath)
    {
        _configPath = configPath;
    }

    public async Task<object> ImportAsync(string component)
    {
        var filePath = Path.Combine(_configPath, $"manifest_{component}.json");
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Manifest file not found: {filePath}");
        }

        var json = await File.ReadAllTextAsync(filePath);
        var manifest = JsonSerializer.Deserialize<object>(json);

        if (manifest == null)
        {
            throw new InvalidOperationException($"Failed to deserialize manifest for {component}");
        }

        return manifest;
    }

    public async Task<T> ImportAsync<T>(string component)
    {
        var filePath = Path.Combine(_configPath, $"manifest_{component}.json");
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Manifest file not found: {filePath}");
        }

        var json = await File.ReadAllTextAsync(filePath);
        var manifest = JsonSerializer.Deserialize<T>(json);

        if (manifest == null)
        {
            throw new InvalidOperationException($"Failed to deserialize manifest for {component}");
        }

        return manifest;
    }
}