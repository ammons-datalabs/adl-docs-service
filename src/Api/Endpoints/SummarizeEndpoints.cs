using Ammons.DataLabs.DocsService.Models;
using Ammons.DataLabs.DocsService.Services;

namespace Ammons.DataLabs.DocsService.Endpoints;

public static class SummarizeEndpoints
{
    public static void MapSummarizeEndpoints(this WebApplication app)
    {
        app.MapPost("/api/summarize", async (SummarizeRequest request, IDocumentSummaryService service) =>
            {
                var summary = await service.SummarizeAsync(request.Text);
                return Results.Ok(summary);
            })
            .WithName("Summarize")
            .WithOpenApi();
    }
}
