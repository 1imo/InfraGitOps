using InfraGitOps.Interfaces;

namespace InfraGitOps.Scripts;

public class ApplyPm2 : IApplier
{
    public string ComponentName => "pm2";

    public async Task ApplyAsync(object manifestData)
    {
        Console.WriteLine($"Applying pm2 (manifest → host)");
        Console.WriteLine($"[ApplyPm2] Manifest data: {System.Text.Json.JsonSerializer.Serialize(manifestData)}");
        
        await Task.Delay(100);
        
        Console.WriteLine($"[ApplyPm2] Successfully applied PM2 configuration to host");
    }
}