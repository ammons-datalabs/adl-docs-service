using Azure;
using Azure.AI.OpenAI;
using Ammons.DataLabs.DocsService.Configuration;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace Ammons.DataLabs.DocsService.Services;

public sealed class AzureOpenAiClient(IOptions<AzureOpenAiOptions> options) : IAzureOpenAiClient
{
    private readonly AzureOpenAiOptions _options = options.Value;

    public async Task<string> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var client = new AzureOpenAIClient(
            new Uri(_options.Endpoint),
            new AzureKeyCredential(_options.ApiKey));

        var chatClient = client.GetChatClient(_options.DeploymentName);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant that summarizes documents concisely."),
            new UserChatMessage(prompt)
        };

        var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);

        return response.Value.Content[0].Text;
    }
}