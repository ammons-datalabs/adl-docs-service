namespace Ammons.DataLabs.DocsService.Models;

public sealed record SummarizeRequest(
    string Text,
    string? Title = null,
    SummaryStyle Style = SummaryStyle.Executive
    );
