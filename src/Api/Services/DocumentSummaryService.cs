namespace Ammons.DataLabs.DocsService.Services;

public sealed class DocumentSummaryService(IAzureOpenAiClient openAiClient) : IDocumentSummaryService
{
    private readonly IAzureOpenAiClient _openAiClient = openAiClient;

    public async Task<string> SummarizeAsync(string text)
    {
        var prompt = $"Please provide a concise summary of the following text:\n\n{text}";
        return await _openAiClient.GetChatCompletionAsync(prompt);
    }
}
