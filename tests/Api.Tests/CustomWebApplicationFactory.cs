using Ammons.DataLabs.DocsService.Models;
using Ammons.DataLabs.DocsService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

/// <summary>
/// Custom WebApplicationFactory that provides test configuration
/// to satisfy AzureOpenAiOptions validation during host startup.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Provide test values for AzureOpenAiOptions to pass startup validation
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureOpenAi:Endpoint"] = "https://test.openai.azure.com/",
                ["AzureOpenAi:ApiKey"] = "test-api-key",
                ["AzureOpenAi:DeploymentName"] = "test-deployment"
            });
        });
        
        // Replace real service with fake for all tests
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDocumentSummaryService));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddScoped<IDocumentSummaryService, FakeDocumentSummaryService>();
        });
    }

    // Fake implementation for testing
    public class FakeDocumentSummaryService: IDocumentSummaryService
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