using System.Net;
using System.Net.Http.Json;
using Ammons.DataLabs.DocsService.Models;
using Ammons.DataLabs.DocsService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;


namespace Api.Tests;

public class SummarizeEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SummarizeEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Summarize_ReturnsOk_WithValidRequest()
    {
        // Arrange
        var request = new { Text = "This is a test document that needs to be summarized." };

        // Act
        var response = await _client.PostAsJsonAsync("/api/summarize", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(summary);
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("OK", content);
    }

    [Fact]
    public async Task Summarize_Returns400_WithEmptyDocument()
    {
        // Arrange
        var request = new { Text = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/summarize", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("empty", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public async Task Summarize_Returns400_WithOnlyWhitespaceDocument()
    {
        // Arrange
        var request = new { Text = "                       " };

        // Act
        var response = await _client.PostAsJsonAsync("/api/summarize", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("empty", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TextEqual16kChars_ReturnsSuccess()
    {
        // Arrange
        var request = new { Text = new string('a', 16384) };

        // Act
        var response = await _client.PostAsJsonAsync("/api/summarize", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(summary);
    }


    [Fact]
    public async Task TextExceeding16kChars_Returns400()
    {
        // Arrange
        var request = new { Text = new string('a', 16384 + 1) };

        // Act
        var response = await _client.PostAsJsonAsync("/api/summarize", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("too long", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }
}
