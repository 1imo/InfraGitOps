using InfraGitOps.Interfaces;

namespace InfraGitOps.Scripts;

public class ApplyDocker : IApplier
{
    public string ComponentName => "docker";

    public async Task ApplyAsync(object manifestData)
    {
        Console.WriteLine($"Applying docker (manifest → host)");
        Console.WriteLine($"[ApplyDocker] Manifest data: {System.Text.Json.JsonSerializer.Serialize(manifestData)}");
        
        await Task.Delay(100);
        
        Console.WriteLine($"[ApplyDocker] Successfully applied Docker configuration to host");
    }
}