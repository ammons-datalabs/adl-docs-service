namespace Ammons.DataLabs.DocsService.Models;

public sealed record SummarizeResponse(
    string Summary,
    int OriginalLength,
    string Model,
    DateTimeOffset GeneratedAt,
    SummaryStyle Style
    );