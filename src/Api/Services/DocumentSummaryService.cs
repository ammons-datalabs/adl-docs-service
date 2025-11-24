using Ammons.DataLabs.DocsService.Models;

namespace Ammons.DataLabs.DocsService.Services;

public sealed class DocumentSummaryService(IAzureOpenAiClient openAiClient, ILogger<DocumentSummaryService> logger)
    : IDocumentSummaryService
{
    public async Task<SummarizeResponse> SummarizeAsync(
        string text,
        SummaryStyle style,
        string? title = null,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(text, style, title);
        var result = await openAiClient.GetChatCompletionAsync(prompt, cancellationToken);
        logger.Log(LogLevel.Information, "summarized document");

        return new SummarizeResponse(
            Summary: result.Summary,
            OriginalLength: text.Length,
            Model: result.Model,
            GeneratedAt: DateTimeOffset.UtcNow,
            Style: style
        );
    }

    private static string BuildPrompt(string text, SummaryStyle style, string? title)
    {
        var styleInstruction = style switch
        {
            SummaryStyle.Executive => "Provide a concise executive summary suitable for leadership review.",
            SummaryStyle.Risks => "Identify and summarize the key risks, concerns, and potential issues.",
            SummaryStyle.Actions => "Extract and list the key action items, decisions, and next steps.",
            SummaryStyle.Technical =>
                "Provide a technical summary focusing on implementation details and specifications.",
            _ => "Provide a concise summary."
        };

        var titleContext = string.IsNullOrWhiteSpace(title)
            ? ""
            : $"Document title: {title}\n\n";

        return $"{styleInstruction}\n\n{titleContext} Text to summarize:\n\n{text}";
    }
}
