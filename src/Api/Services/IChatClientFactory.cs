using OpenAI.Chat;

namespace Ammons.DataLabs.DocsService.Services;

public interface IChatClientFactory
{
    ChatClient Create(string deploymentName);
}