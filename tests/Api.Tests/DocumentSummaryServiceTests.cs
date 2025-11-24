using Ammons.DataLabs.DocsService.Models;
using Ammons.DataLabs.DocsService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Api.Tests;

public class DocumentSummaryServiceTests
{
    [Fact]
    public async Task SummarizeAsync_WithExecutiveStyle_BuildsCorrectPrompt()
    {
        // Arrange
        var mockClient = new Mock<IAzureOpenAiClient>();
        const string expectedSummary = "Executive summary from AI";
        
        mockClient
            .Setup(x => x.GetChatCompletionAsync(
            It.Is<string>(p => p.Contains("executive summary")),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestResult("Executive summary from AI"));

        var service = new DocumentSummaryService(mockClient.Object, NullLogger<DocumentSummaryService>.Instance);
        
        // Act
        var result = await service.SummarizeAsync(
            "Some document text",
            SummaryStyle.Executive);
        
        // Assert
        Assert.Equal(expectedSummary, result.Summary);
        Assert.Equal(18, result.OriginalLength); // "Som document text".Length
        Assert.Equal(SummaryStyle.Executive, result.Style);
        Assert.Equal("gpt-4o-mini", result.Model);
        mockClient.Verify(x => x.GetChatCompletionAsync(
            It.Is<string>(prompt => prompt.Contains("executive summary")),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(SummaryStyle.Executive, "executive summary")]
    [InlineData(SummaryStyle.Risks, "risks")]
    [InlineData(SummaryStyle.Actions, "action items")]
    [InlineData(SummaryStyle.Technical, "technical summary")]
    public async Task SummarizeAsync_WithDifferentStyles_BuildsAppropriatePrompts(
        SummaryStyle style,
        string expectedKeyword)
    {
        // Arrange
        var mockClient = new Mock<IAzureOpenAiClient>();
        mockClient
            .Setup(x => x.GetChatCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestResult("Executive summary from AI"));
        
        var service = new DocumentSummaryService(mockClient.Object, NullLogger<DocumentSummaryService>.Instance);
        
        // Act
        await service.SummarizeAsync("test text", style);
        
        // Assert
        mockClient.Verify(x => x.GetChatCompletionAsync(
                It.Is<string>(p => p.Contains(expectedKeyword, StringComparison.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SummarizeAsync_WithTitle_IncludesTitleInPrompt()
    {
        // Arrange
        var mockClient = new Mock<IAzureOpenAiClient>();
        mockClient
            .Setup(x => x.GetChatCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestResult("Executive summary from AI"));

        var service = new DocumentSummaryService(mockClient.Object, NullLogger<DocumentSummaryService>.Instance);
        
        // Act
        await service.SummarizeAsync(
            "document text",
            SummaryStyle.Executive,
            title: "Q4 Financial Report");
        
        // Assert
        mockClient.Verify(x => x.GetChatCompletionAsync(
            It.Is<string>(p => p.Contains("Q4 Financial Report")),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SummarizeAsync_SetsCorrectResponseMetadata()
    {
        // Arrange
        var mockClient = new Mock<IAzureOpenAiClient>();
        mockClient
            .Setup(x => x.GetChatCompletionAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestResult("Executive summary from AI"));
        
        var service = new DocumentSummaryService(mockClient.Object, NullLogger<DocumentSummaryService>.Instance);
        const string testText = "This is a test document";
        var beforeTime = DateTimeOffset.UtcNow;
        
        // Act
        var result = await service.SummarizeAsync(testText, SummaryStyle.Technical);
        var afterTime = DateTimeOffset.UtcNow;
        
        // Assert
        Assert.Equal("Executive summary from AI", result.Summary);
        Assert.Equal(testText.Length, result.OriginalLength);
        Assert.Equal("gpt-4o-mini", result.Model);
        Assert.Equal(SummaryStyle.Technical, result.Style);
        Assert.InRange(result.GeneratedAt, beforeTime, afterTime);
    }

    [Fact]
    public async Task SummarizeAsync_LogsInformation()
    {
        // Arrange
        var mockClient = new Mock<IAzureOpenAiClient>();
        mockClient
            .Setup(x => x.GetChatCompletionAsync(It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestResult("Summary"));
        
        var logger = new TestLogger<DocumentSummaryService>();
        var service = new DocumentSummaryService(mockClient.Object, logger);
        

        // Act
        await service.SummarizeAsync("Test text", SummaryStyle.Executive);
        
        // Assert
        var infoLog = logger.Entries.FirstOrDefault(e => e.LogLevel == LogLevel.Information);
        Assert.NotNull(infoLog);
        Assert.Contains("summarize", infoLog.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    // Helper method
    private static ChatCompletionResult CreateTestResult(string summary = "Test summary") =>
        new ChatCompletionResult(
            Summary: summary,
            Model: "gpt-4o-mini");
    
}

