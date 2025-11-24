using System.Net;
using System.Net.Http.Json;
using Ammons.DataLabs.DocsService.Middleware;
using Ammons.DataLabs.DocsService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.Tests;

public sealed class DocumentSummaryExceptionMiddlewareTests
{

    [Fact]
    public async Task When_DocumentSummaryException_Throw_Returns_503_And_Logs()
    {
        // Arrange
        var logger = new TestLogger<DocumentSummaryExceptionMiddleware>();

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<ILogger<DocumentSummaryExceptionMiddleware>>(logger);
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<DocumentSummaryExceptionMiddleware>();
                        app.Run(_ => throw new DocumentSummaryException("Upstream AI error"));
                    });
            })
            .StartAsync();

        var client = host.GetTestClient();
        
        // Act
        var response = await client.GetAsync("/");
        var body = await  response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert HTTP
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(503, body.Status);
        Assert.Equal("Document summarization unavailable", body.Title);
        Assert.Contains("Upstream AI error", body.Detail);
        
        // Assert logs
        var errorLog = logger.Entries.SingleOrDefault(e => e.LogLevel == LogLevel.Error);
        Assert.NotNull(errorLog);
        Assert.IsType<DocumentSummaryException>(errorLog.Exception);
        Assert.Contains("Document summary failed", errorLog.Message);
    }
    
    
    [Fact]
    public async Task When_UnexpectedException_Thrown_Returns_500_And_Logs()
    {
        // Arrange
        var logger = new TestLogger<DocumentSummaryExceptionMiddleware>();

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<ILogger<DocumentSummaryExceptionMiddleware>>(logger);
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<DocumentSummaryExceptionMiddleware>();
                        app.Run(_ => throw new InvalidOperationException("Unexpected boom"));
                    });
            })
            .StartAsync();

        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/");
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert HTTP
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(500, body.Status);
        Assert.Equal("Unexpected error", body.Title);
        Assert.Contains("unexpected error occurred", body.Detail);

        // Assert logs
        var errorLog = logger.Entries.SingleOrDefault(e => e.LogLevel == LogLevel.Error);
        Assert.NotNull(errorLog);
        Assert.IsType<InvalidOperationException>(errorLog.Exception);
        Assert.Contains("unhandled exception", errorLog.Message);
    }
}