using Ammons.DataLabs.DocsService.Models;

namespace Ammons.DataLabs.DocsService.Services;

public interface IDocumentSummaryService
{
    Task<SummarizeResponse> SummarizeAsync(
        string text,
        SummaryStyle style,
        string? title = null,
        CancellationToken cancellationToken = default);
}
