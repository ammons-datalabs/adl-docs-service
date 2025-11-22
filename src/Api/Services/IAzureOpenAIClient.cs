namespace Ammons.DataLabs.DocsService.Services;

public interface IAzureOpenAiClient
{
    Task<string> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default);
}