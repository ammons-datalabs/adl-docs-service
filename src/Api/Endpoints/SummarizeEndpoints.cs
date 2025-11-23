using System.Reflection;
using Ammons.DataLabs.DocsService.Models;
using Ammons.DataLabs.DocsService.Services;

namespace Ammons.DataLabs.DocsService.Endpoints;

public static class SummarizeEndpoints
{
    public static void MapSummarizeEndpoints(this WebApplication app)
    {
        app.MapPost("/api/summarize", async (
                SummarizeRequest request,
                IDocumentSummaryService service,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.Text))
                {
                    return Results.Problem(
                        title: "Invalid request",
                        detail: "Text cannot be empty or whitespace",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                if (request.Text.Length > 16384)
                {
                    return Results.Problem(
                        title: "Request too long",
                        detail: "Text too long, cannot be > 16k characters long",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }
                var response = await service.SummarizeAsync(
                    request.Text,
                    request.Style,
                    request.Title,
                    cancellationToken);
                
                return Results.Ok(response);
            })
            .WithName("Summarize")
            .WithOpenApi();
    }
}
