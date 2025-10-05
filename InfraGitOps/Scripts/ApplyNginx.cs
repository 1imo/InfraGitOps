using InfraGitOps.Interfaces;

namespace InfraGitOps.Scripts;

public class ApplyNginx : IApplier
{
    public string ComponentName => "nginx";

    public async Task ApplyAsync(object manifestData)
    {
        Console.WriteLine($"Applying nginx (manifest → host)");
        Console.WriteLine($"[ApplyNginx] Manifest data: {System.Text.Json.JsonSerializer.Serialize(manifestData)}");
        
        await Task.Delay(100);
        
        Console.WriteLine($"[ApplyNginx] Successfully applied Nginx configuration to host");
    }
}