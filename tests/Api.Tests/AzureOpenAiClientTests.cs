using Ammons.DataLabs.DocsService.Configuration;
using Ammons.DataLabs.DocsService.Services;
using Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OpenAI.Chat;

namespace Api.Tests;

public class AzureOpenAiClientTests
{
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
        var chatClient = new ChatClientFactory(options);

        var client = new AzureOpenAiClient(options, chatClient, logger);
        
        // Act & Assert - Verify exception is thrown
        var exception = await Assert.ThrowsAsync<UpstreamServiceException>(() =>
            client.GetChatCompletionAsync("summarise this text", CancellationToken.None));

        Assert.Contains("Failed to generate summary", exception.Message);
        
        // Assert logs
        var errorLog = logger.Entries.FirstOrDefault(e => e.LogLevel == LogLevel.Error);
        Assert.NotNull(errorLog);
        Assert.NotNull(errorLog.Exception);
        Assert.Contains("Azure OpenAI error", errorLog.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetCompletionAsync_RequestFailedException_ThrowsUpstreamServiceException()
    {
        // Arrange
        var logger = new TestLogger<AzureOpenAiClient>();
        var options = Options.Create(new AzureOpenAiOptions
        {
            Endpoint = "https://test.openai.azure.com",
            ApiKey = "test-key",
            DeploymentName =  "test-deployment"
        });

        var mockFactory = new Mock<IChatClientFactory>();
        var mockChatClient = new Mock<ChatClient>();
        
        mockChatClient
            .Setup(c => c.CompleteChatAsync(It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(429, "Rate limited"));

        mockFactory
            .Setup(f => f.Create(It.IsAny<string>()))
            .Returns(mockChatClient.Object);

        var client = new AzureOpenAiClient(options, mockFactory.Object, logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UpstreamServiceException>(() =>
            client.GetChatCompletionAsync("test prompt", CancellationToken.None));

        Assert.Contains("Azure OpenAI error", exception.Message);
    }
}