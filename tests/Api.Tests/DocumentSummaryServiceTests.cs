using Ammons.DataLabs.DocsService.Services;
using Moq;

namespace Api.Tests;

public class DocumentSummaryServiceTests
{
    [Fact]
    public async Task SummarizeAsync_CallsAzureOpenAI_ReturnsSummary()
    {
        // Arrange
        var mockClient = new Mock<IAzureOpenAiClient>();
        var expectedSummary = "This is a test summary from Azure OpenAI";
        
        mockClient
            .Setup(x => x.GetChatCompletionAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        var service = new DocumentSummaryService(mockClient.Object);
        
        // Act
        var result = await service.SummarizeAsync("Some long document text to summarize");
        
        // Assert
        Assert.Equal(expectedSummary, result);
        mockClient.Verify(x => x.GetChatCompletionAsync(
            It.Is<string>(prompt => prompt.Contains("summarize")),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

