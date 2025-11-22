namespace Ammons.DataLabs.DocsService.Services;

public sealed class DocumentSummaryService(IAzureOpenAiClient openAiClient) : IDocumentSummaryService
{
    private readonly IAzureOpenAiClient _openAiClient = openAiClient;

    public Task<string> SummarizeAsync(string text)
    {
        // TODO: Implement Azure OpenAI integration
        return Task.FromResult($"Summary of text with {text.Length} characters (stubbed)");
    }
}
