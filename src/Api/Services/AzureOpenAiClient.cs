using Azure;
using System.ClientModel;
using Azure.AI.OpenAI;
using Ammons.DataLabs.DocsService.Configuration;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace Ammons.DataLabs.DocsService.Services;

public sealed class AzureOpenAiClient: IAzureOpenAiClient
{
    private readonly ChatClient _chatClient;
    private readonly string _deploymentName;
    private readonly ILogger<AzureOpenAiClient> _logger;

    public AzureOpenAiClient(
        IOptions<AzureOpenAiOptions> options,
        IChatClientFactory chatClientFactory,
        ILogger<AzureOpenAiClient> logger)
    {
        var config = options.Value;
        _logger = logger;
        _deploymentName = config.DeploymentName;
        _chatClient = chatClientFactory.Create(_deploymentName);
    }
    public async Task<ChatCompletionResult> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calling Azure OpenAI deployment {deployment} for chat completions.",
                _deploymentName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant that summarizes documents concisely."),
                new UserChatMessage(prompt)
            };

            var response = await _chatClient.CompleteChatAsync(
                messages, cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully received response from Azure OpenAI deployment {Deployment}.",
                _deploymentName);

            return new ChatCompletionResult(
                Summary: response.Value.Content[0].Text,
                Model: _deploymentName);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, 
                "Azure OpenAI request failed for {DeploymentName}. Status: {Status}, Error: {ErrorCode}.", 
                _deploymentName,
                ex.Status,
                ex.ErrorCode);
            throw new UpstreamServiceException("Failed to generate summary due to Azure OpenAI error on request.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI error on request for {DeploymentName}.", _deploymentName);
            throw new UpstreamServiceException("Failed to generate summary due to unexpected error.", ex);
        }
        
    }
}