using InfraGitOps.Workflows;
using InfraGitOps.Interfaces;
using InfraGitOps.Scripts;
using InfraGitOps.Models;
using Moq;
using Xunit;

namespace InfraGitOps.Tests;

public class WorkflowOrchestrationTests
{
    [Fact]
    public async Task DockerWorkflowOrchestration_ExecutesSuccessfully()
    {
        var applier = new ApplyDocker();
        var transformers = new List<ITransformer>();

        var workflow = new DockerWorkflowOrchestration(applier, transformers);
        var manifest = new DockerManifest { Version = 1 };

        await workflow.ExecuteAsync(manifest);

        Assert.Equal("docker", workflow.ComponentName);
    }

    [Fact]
    public async Task Pm2WorkflowOrchestration_ExecutesSuccessfully()
    {
        var applier = new ApplyPm2();
        var transformers = new List<ITransformer>();

        var workflow = new Pm2WorkflowOrchestration(applier, transformers);
        var manifest = new Pm2Manifest { Version = 1 };

        await workflow.ExecuteAsync(manifest);

        Assert.Equal("pm2", workflow.ComponentName);
    }

    [Fact]
    public async Task NginxWorkflowOrchestration_ExecutesSuccessfully()
    {
        var applier = new ApplyNginx();
        var transformers = new List<ITransformer>();

        var workflow = new NginxWorkflowOrchestration(applier, transformers);
        var manifest = new NginxManifest { Version = 1 };

        await workflow.ExecuteAsync(manifest);

        Assert.Equal("nginx", workflow.ComponentName);
    }

    [Fact]
    public async Task UfwWorkflowOrchestration_ExecutesSuccessfully()
    {
        var applier = new ApplyUfw();
        var transformers = new List<ITransformer>();

        var workflow = new UfwWorkflowOrchestration(applier, transformers);
        var manifest = new UfwManifest { Version = 1 };

        await workflow.ExecuteAsync(manifest);

        Assert.Equal("ufw", workflow.ComponentName);
    }

    [Fact]
    public async Task ImagesWorkflowOrchestration_ExecutesSuccessfully()
    {
        var applier = new ApplyImages();
        var transformers = new List<ITransformer>();

        var workflow = new ImagesWorkflowOrchestration(applier, transformers);
        var manifest = new ImagesManifest { Version = 1 };

        await workflow.ExecuteAsync(manifest);

        Assert.Equal("images", workflow.ComponentName);
    }

    [Fact]
    public void AllWorkflowOrchestrations_HaveCorrectComponentNames()
    {
        var dockerWorkflow = new DockerWorkflowOrchestration(new ApplyDocker(), new List<ITransformer>());
        var pm2Workflow = new Pm2WorkflowOrchestration(new ApplyPm2(), new List<ITransformer>());
        var nginxWorkflow = new NginxWorkflowOrchestration(new ApplyNginx(), new List<ITransformer>());
        var ufwWorkflow = new UfwWorkflowOrchestration(new ApplyUfw(), new List<ITransformer>());
        var imagesWorkflow = new ImagesWorkflowOrchestration(new ApplyImages(), new List<ITransformer>());

        Assert.Equal("docker", dockerWorkflow.ComponentName);
        Assert.Equal("pm2", pm2Workflow.ComponentName);
        Assert.Equal("nginx", nginxWorkflow.ComponentName);
        Assert.Equal("ufw", ufwWorkflow.ComponentName);
        Assert.Equal("images", imagesWorkflow.ComponentName);
    }
}
