namespace Ammons.DataLabs.DocsService.Services;

public sealed class DocumentSummaryException(string message, Exception? inner = null) : Exception(message, inner);