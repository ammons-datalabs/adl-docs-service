using Ammons.DataLabs.DocsService.Configuration;
using Ammons.DataLabs.DocsService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit.Sdk;

namespace Api.Tests;

public class AzureOpenAiClientIntegrationTests
{
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
        
        Assert.NotEmpty(result);
        Assert.Contains("hello", result, StringComparison.OrdinalIgnoreCase);
    }
}