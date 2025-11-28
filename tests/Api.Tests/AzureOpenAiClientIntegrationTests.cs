using Ammons.DataLabs.DocsService.Configuration;
using Ammons.DataLabs.DocsService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace Api.Tests;

public class AzureOpenAiClientIntegrationTests(ITestOutputHelper output)
{
    [Fact(Skip = "Manual verification only - requires user secrets")]
    public void Debug_Configuration()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<AzureOpenAiClientIntegrationTests>()
            .Build();

        var endpoint = configuration["AzureOpenAi:Endpoint"];
        var apiKey = configuration["AzureOpenAi:ApiKey"];
        var deploymentName = configuration["AzureOpenAi:DeploymentName"];
        
        output.WriteLine($"Endpoint: {endpoint}");
        output.WriteLine($"ApiKey: {(string.IsNullOrEmpty(apiKey) ? "NOT Set": "SET")}");
        output.WriteLine($"DeploymentName: {deploymentName}");

        Assert.NotNull(endpoint);
    }
    
    [Fact(Skip = "Integration test requiring live Azure OpenAI with configured secrets")]
    public async Task GetChatCompletionAsync_WithValidPrompt_ReturnsResponse()
    {
        // Arrange - load configuration from user secrets
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<AzureOpenAiClientIntegrationTests>()
            .Build();

        var azureOpenAiConfig = configuration.GetSection(AzureOpenAiOptions.SectionName);
        var configValues = azureOpenAiConfig.Get<AzureOpenAiOptions>();
        
        // Skip test if credentials aren't configured
        if (configValues == null ||
            string.IsNullOrEmpty(configValues.Endpoint) ||
            configValues.Endpoint == $"your-endpoint")
        {
            // Skip is credentials aren't configured
            Assert.Fail("Azure OpenAI credentials not configured");
            return;
        }
        
        var options = Options.Create(configValues);
        var chatClient = new ChatClientFactory(options);
        var client = new AzureOpenAiClient(options, chatClient, NullLogger<AzureOpenAiClient>.Instance);
        
        // Act
        var result = await client.GetChatCompletionAsync("Say hello");
        
        Assert.False(string.IsNullOrWhiteSpace(result.Summary));
        Assert.Contains("hello", result.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(result.Model);
    }
}