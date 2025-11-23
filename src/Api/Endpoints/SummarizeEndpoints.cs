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
