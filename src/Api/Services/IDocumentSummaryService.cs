namespace Ammons.DataLabs.DocsService.Services;

public interface IDocumentSummaryService
{
    Task<string> SummarizeAsync(string text);
}
