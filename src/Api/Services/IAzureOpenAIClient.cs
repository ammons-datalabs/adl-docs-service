namespace Ammons.DataLabs.DocsService.Services;

public interface IAzureOpenAiClient
{
    Task<ChatCompletionResult> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default);
}

public record ChatCompletionResult(string Summary, string Model);