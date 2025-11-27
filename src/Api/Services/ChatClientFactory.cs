using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using Ammons.DataLabs.DocsService.Configuration;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace Ammons.DataLabs.DocsService.Services;

public sealed class ChatClientFactory : IChatClientFactory
{
    private readonly AzureOpenAIClient _client;

    public ChatClientFactory(IOptions<AzureOpenAiOptions> options)
    {
        var opts = options.Value;
        _client = new AzureOpenAIClient(
            new Uri(opts.Endpoint),
            new ApiKeyCredential(opts.ApiKey));
    }

    public ChatClient Create(string deploymentName) => _client.GetChatClient(deploymentName);
}