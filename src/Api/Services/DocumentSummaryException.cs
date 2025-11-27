namespace Ammons.DataLabs.DocsService.Services;

public sealed class DocumentSummaryException(string message, int statusCode = 500, Exception? innerException = null)
    : Exception(message, innerException)
{
    public int StatusCode { get;  } = statusCode;
}