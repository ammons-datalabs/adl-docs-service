using System.Net;
using System.Net.Http.Json;
using Ammons.DataLabs.DocsService.Services;
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
    
    // Fake implementation for testing
    private class FakeDocumentSummaryService: IDocumentSummaryService
    {
        public Task<string> SummarizeAsync(string text)
        {
            return Task.FromResult($"Fake summary of {text.Length} characters");
        }
    }
}
