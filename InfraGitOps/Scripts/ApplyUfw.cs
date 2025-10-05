using InfraGitOps.Interfaces;

namespace InfraGitOps.Scripts;

public class ApplyUfw : IApplier
{
    public string ComponentName => "ufw";

    public async Task ApplyAsync(object manifestData)
    {
        Console.WriteLine($"Applying ufw (manifest → host)");
        Console.WriteLine($"[ApplyUfw] Manifest data: {System.Text.Json.JsonSerializer.Serialize(manifestData)}");
        
        await Task.Delay(100);
        
        Console.WriteLine($"[ApplyUfw] Successfully applied UFW configuration to host");
    }
}
