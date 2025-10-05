using InfraGitOps.Interfaces;
using System.Text.Json;

namespace InfraGitOps.UseCases;

public class ExportDevice
{
    private readonly IEnumerable<IExporter> _exporters;
    private readonly string _configPath;

    public ExportDevice(IEnumerable<IExporter> exporters, string configPath)
    {
        _exporters = exporters;
        _configPath = configPath;
    }

    public async Task ExportAllAsync()
    {
        Console.WriteLine("Exporting host state to all manifests (host → manifest)");

        foreach (var exporter in _exporters)
        {
            try
            {
                Console.WriteLine($"Exporting {exporter.ComponentName}");
                var manifestData = await exporter.ExportAsync();
                await SaveManifestAsync(exporter.ComponentName, manifestData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting {exporter.ComponentName}: {ex.Message}");
            }
        }

        Console.WriteLine("Export completed");
    }

    private async Task SaveManifestAsync(string component, object manifestData)
    {
        var filePath = Path.Combine(_configPath, $"manifest_{component}.json");
        var json = JsonSerializer.Serialize(manifestData, 
            new JsonSerializerOptions { WriteIndented = true });
        
        await File.WriteAllTextAsync(filePath, json);
    }
}