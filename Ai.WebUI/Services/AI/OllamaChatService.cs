using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ai.WebUI.Services.AI;

public class OllamaChatService(Kernel kernel, IHttpClientFactory httpClientFactory)
{
    public async IAsyncEnumerable<string> StreamAsync(
        ChatHistory history,
        string modelId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var settings = new PromptExecutionSettings { ModelId = modelId };

        await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(
            history, settings, kernel, cancellationToken))
        {
            if (chunk.Content is not null)
                yield return chunk.Content;
        }
    }

    public async Task<string> CompleteAsync(
        ChatHistory history,
        string modelId,
        CancellationToken cancellationToken = default)
    {
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var settings = new PromptExecutionSettings { ModelId = modelId };
        var result = await chatService.GetChatMessageContentAsync(history, settings, kernel, cancellationToken);
        return result.Content ?? string.Empty;
    }

    public async Task<List<string>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("ollama");
        var response = await client.GetFromJsonAsync<OllamaTagsResponse>("api/tags", cancellationToken);
        return response?.Models.Select(m => m.Name).ToList() ?? [];
    }

    private record OllamaTagsResponse(
        [property: JsonPropertyName("models")] List<OllamaModel> Models);

    private record OllamaModel(
        [property: JsonPropertyName("name")] string Name);
}
