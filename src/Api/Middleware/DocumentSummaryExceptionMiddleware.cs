using Ammons.DataLabs.DocsService.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ammons.DataLabs.DocsService.Middleware;

public class DocumentSummaryExceptionMiddleware(
    RequestDelegate next,
    ILogger<DocumentSummaryExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DocumentSummaryException ex)
        {
            logger.LogError(ex, "Document summary failed for {Path}", context.Request.Path);
            await WriteProblemDetailsAsync(
                context,
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Document summarization unavailable",
                detail: ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred while processing {Path}", context.Request.Path);
            await WriteProblemDetailsAsync(
                context,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unexpected error",
                detail: "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }
        
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = "about:blank"
        };
        
        await context.Response.WriteAsJsonAsync(problem);
    }
}