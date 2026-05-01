using Microsoft.SemanticKernel.ChatCompletion;

namespace Ai.WebUI.Services.AI;

public interface IOllamaChatService
{
    IAsyncEnumerable<string> StreamAsync(
        ChatHistory history,
        string modelId,
        CancellationToken cancellationToken = default);

    Task<string> CompleteAsync(
        ChatHistory history,
        string modelId,
        CancellationToken cancellationToken = default);
}
