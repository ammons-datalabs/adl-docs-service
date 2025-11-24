using Azure;
using Azure.AI.OpenAI;
using Ammons.DataLabs.DocsService.Configuration;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace Ammons.DataLabs.DocsService.Services;

public sealed class AzureOpenAiClient(IOptions<AzureOpenAiOptions> options, ILogger<AzureOpenAiClient> logger) : IAzureOpenAiClient
{
    private readonly AzureOpenAiOptions _options = options.Value;
    private readonly ILogger<AzureOpenAiClient> _logger = logger;

    public async Task<ChatCompletionResult> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calling Azure OpenAI deployment {deployment} for chat completions.",
                _options.DeploymentName);
            var client = new AzureOpenAIClient(
                new Uri(_options.Endpoint),
                new AzureKeyCredential(_options.ApiKey));

            var chatClient = client.GetChatClient(_options.DeploymentName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant that summarizes documents concisely."),
                new UserChatMessage(prompt)
            };

            var response = await chatClient.CompleteChatAsync(
                messages, cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully received response from Azure OpenAI deployment {Deployment}.",
                _options.DeploymentName);

            return new ChatCompletionResult(
                Summary: response.Value.Content[0].Text,
                Model: _options.DeploymentName);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, 
                "Azure OpenAI request failed for {DeploymentName}. Status: {Status}, Error: {ErrorCode}.", 
                _options.DeploymentName,
                ex.Status,
                ex.ErrorCode);
            throw new DocumentSummaryException("Failed to generate summary due to Azure OpenAI error on request.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI error on request for {DeploymentName}.", _options.DeploymentName);
            throw new DocumentSummaryException("Failed to generate summary due to unexpected error.", ex);
        }
        
    }
}