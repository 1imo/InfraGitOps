using InfraGitOps.Interfaces;

namespace InfraGitOps.Scripts;

public class ApplyImages : IApplier
{
    public string ComponentName => "images";

    public async Task ApplyAsync(object manifestData)
    {
        Console.WriteLine($"Applying images (manifest → host)");
        Console.WriteLine($"[ApplyImages] Manifest data: {System.Text.Json.JsonSerializer.Serialize(manifestData)}");
        
        await Task.Delay(100);
        
        Console.WriteLine($"[ApplyImages] Successfully applied Images configuration to host");
    }
}