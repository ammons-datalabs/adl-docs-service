using System.Net;
using System.Net.Http.Json;
using Ammons.DataLabs.DocsService.Models;
using Ammons.DataLabs.DocsService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;


namespace Api.Tests;

public class SummarizeEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SummarizeEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the original DocumentSummaryService registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDocumentSummaryService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                // Replace the real service with a fake one for integration tests
                services.AddScoped<IDocumentSummaryService, FakeDocumentSummaryService>();
            });
        });
    }

    [Fact]
    public async Task Summarize_ReturnsOk_WithValidRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { Text = "This is a test document that needs to be summarized." };

        // Act
        var response = await client.PostAsJsonAsync("/api/summarize", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(summary);
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("OK", content);
    }

    [Fact]
    public async Task Summarize_Returns400_WithEmptyDocument()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { Text = "" };

        // Act
        var response = await client.PostAsJsonAsync("/api/summarize", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("empty", problem!.Detail, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public async Task Summarize_Returns400_WithOnlyWhitespaceDocument()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { Text = "                       " };

        // Act
        var response = await client.PostAsJsonAsync("/api/summarize", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("empty", problem!.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TextEqual16kChars_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { Text = new string('a', 16384) };

        // Act
        var response = await client.PostAsJsonAsync("/api/summarize", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(summary);
    }


    [Fact]
    public async Task TextExceeding16kChars_Returns400()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { Text = new string('a', 16384 + 1) };

        // Act
        var response = await client.PostAsJsonAsync("/api/summarize", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("too long", problem!.Detail, StringComparison.OrdinalIgnoreCase);
    }


    // Fake implementation for testing
    private class FakeDocumentSummaryService: IDocumentSummaryService
    {
        public Task<SummarizeResponse> SummarizeAsync(
            string text,
            SummaryStyle style,
            string? title = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SummarizeResponse(
                Summary: $"Fake summary of {text.Length} characters",
                OriginalLength: text.Length,
                Model: "fake-model",
                GeneratedAt: DateTimeOffset.UtcNow,
                Style: style
                ));
        }
    }
}
