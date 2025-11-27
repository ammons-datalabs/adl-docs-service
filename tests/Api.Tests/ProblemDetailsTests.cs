using System.Net;
using System.Net.Http.Json;
using Ammons.DataLabs.DocsService.Models;
using Ammons.DataLabs.DocsService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

public class ProblemDetailsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task UpstreamServiceFailure_Returns503_WithProblemDetails()
    {
        // Arrange - Mock service that throws UpstreamServiceException
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDocumentSummaryService));
                if (descriptor != null) services.Remove(descriptor);

                services.AddScoped<IDocumentSummaryService, FakeUpstreamFailureService>();
            });
        }).CreateClient();

        var request = new { Text = "Test document" };
        
        // Act
        var response = await client.PostAsJsonAsync("/api/summarize", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(503, problem.Status);
        Assert.Equal("Service unavailable", problem.Title);
        Assert.Contains("temporarily unavailable", problem.Detail ?? "", StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(problem.Extensions["traceId"]);
    }
    
    [Fact]
    public async Task UpstreamServiceUnexpectedError_Returns500_WithProblemDetails()
    {
        // Arrange - Mock service that throws UpstreamServiceException
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDocumentSummaryService));
                if (descriptor != null) services.Remove(descriptor);

                services.AddScoped<IDocumentSummaryService, FakeUnexpectedFailureService>();
            });
        }).CreateClient();

        var request = new { Text = "Test document" };
        
        // Act
        var response = await client.PostAsJsonAsync("/api/summarize", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal("An unexpected error occurred", problem.Title);
        Assert.Contains("Simulated unexpected error", problem.Detail ?? "", StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(problem.Extensions["traceId"]);
    }
    
    // Fake service that simulates upstream failure
    private class FakeUpstreamFailureService: IDocumentSummaryService
    {
        public Task<SummarizeResponse> SummarizeAsync(string text, SummaryStyle style, string? title = null,
            CancellationToken cancellationToken = default)
        {
            throw new DocumentSummaryException(
                "The AI service is temporarily unavailable. Please try later.",
                statusCode: 503,
                innerException: new UpstreamServiceException("Simulated upstream failures"));
        }
    }
    
    // Fake service that throw unexpected exception
    private class FakeUnexpectedFailureService : IDocumentSummaryService
    {
        public Task<SummarizeResponse> SummarizeAsync(string text, SummaryStyle style, string? title = null,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated unexpected error");
        }
    }
}