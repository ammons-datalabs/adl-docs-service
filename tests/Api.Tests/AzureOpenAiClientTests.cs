using Ammons.DataLabs.DocsService.Configuration;
using Ammons.DataLabs.DocsService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Tests;

public class AzureOpenAiClientTests
{
    [Fact]
    public async Task GetChatCompletionAsync_LogsError_WhenAzureOpenAiFails()
    {
        // Arrange
        var logger = new TestLogger<AzureOpenAiClient>();
        var options = Options.Create(new AzureOpenAiOptions
        {
            Endpoint = "https://test.openai.azure.com",
            ApiKey = "test-key",
            DeploymentName =  "test-deployment"
        });

        var client = new AzureOpenAiClient(options, logger);
        
        // Act & Assert - Verify exception is thrown
        var exception = await Assert.ThrowsAsync<DocumentSummaryException>(() =>
            client.GetChatCompletionAsync("test"));
        
        // Assert
        var errorlog = logger.Entries.FirstOrDefault(e => e.LogLevel == LogLevel.Error);
        Assert.NotNull(errorlog);
        Assert.NotNull(errorlog.Exception);
        Assert.Contains("Azure OpenAI", errorlog.Message, StringComparison.OrdinalIgnoreCase);
        
        // Assert - Verify exception details
        Assert.Contains("Failed to generate summary", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(exception.InnerException);
    }
    
    [Fact]
    public void AzureOpenAiClient_AcceptsLogger()
    {
        // Arrange
        var logger = new TestLogger<AzureOpenAiClient>();
        var options = Options.Create(new AzureOpenAiOptions
        {
            Endpoint = "https://test.openai.azure.com",
            ApiKey = "test-key",
            DeploymentName =  "test-deployment"
        });
        
        // Act
        var client = new AzureOpenAiClient(options, logger);
        
        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public async Task GetCompletionAsync_Logs_And_Wraps_ClientResultException()
    {
        var logger = new TestLogger<AzureOpenAiClient>();
        var options = Options.Create(new AzureOpenAiOptions
        {
            Endpoint = "https://test.openai.azure.com",
            ApiKey = "test-key",
            DeploymentName =  "test-deployment"
        });

        var client = new AzureOpenAiClient(options, logger);
        
        // Act & Assert - Verify exception is thrown
        var exception = await Assert.ThrowsAsync<DocumentSummaryException>(() =>
            client.GetChatCompletionAsync("summarise this text", CancellationToken.None));

        Assert.Contains("Failed to generate summary", exception.Message);
        
        // Assert logs
        var errorLog = logger.Entries.FirstOrDefault(e => e.LogLevel == LogLevel.Error);
        Assert.NotNull(errorLog);
        Assert.NotNull(errorLog.Exception);
        Assert.Contains("Azure OpenAI error", errorLog.Message, StringComparison.OrdinalIgnoreCase);

    }
}