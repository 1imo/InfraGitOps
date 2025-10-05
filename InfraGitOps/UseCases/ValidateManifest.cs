using System.Text.Json;

namespace InfraGitOps.UseCases;

public class ValidateManifest
{
    public async Task<bool> ValidateAsync(object manifestData, string component)
    {
        try
        {
            if (manifestData == null)
            {
                Console.WriteLine($"Validation failed for {component}: Manifest is null");
                return false;
            }

            var json = JsonSerializer.Serialize(manifestData);
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            if (!element.TryGetProperty("Version", out var versionProperty))
            {
                Console.WriteLine($"Validation failed for {component}: Missing Version property");
                return false;
            }

            Console.WriteLine($"Validation passed for {component}");
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Validation error for {component}: {ex.Message}");
            return false;
        }
    }
}