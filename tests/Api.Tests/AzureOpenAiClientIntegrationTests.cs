using Ammons.DataLabs.DocsService.Configuration;
using Ammons.DataLabs.DocsService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace Api.Tests;

public class AzureOpenAiClientIntegrationTests(ITestOutputHelper output)
{
    [Fact]
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
    
    [Fact]
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
            return;
        }
        
        var options = Options.Create(configValues);
        var client = new AzureOpenAiClient(options);
        
        // Act
        var result = await client.GetChatCompletionAsync("Say hello");
        
        Assert.Equal("Hello! How can I assist you today?", result.Summary);
        Assert.Equal("gpt-4o-mini", result.Model);
    }
}