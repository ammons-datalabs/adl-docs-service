namespace Ammons.DataLabs.DocsService.Services;

public sealed class UpstreamServiceException(string message, Exception? innterException = null)
    : Exception(message, innterException);