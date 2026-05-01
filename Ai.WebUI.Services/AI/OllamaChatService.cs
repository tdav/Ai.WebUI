using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ai.WebUI.Services.AI;

public class OllamaChatService(IHttpClientFactory httpClientFactory) : IOllamaChatService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async IAsyncEnumerable<string> StreamAsync(
        ChatHistory history,
        string modelId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("ollama");
        var request = BuildRequest(history, modelId, stream: true);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/chat")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        using var response = await client.SendAsync(
            httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
                break;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string? content;
            bool done;
            try
            {
                (content, done) = ParseChunk(line);
            }
            catch (JsonException)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(content))
                yield return content;

            if (done)
                break;
        }
    }

    public async Task<string> CompleteAsync(
        ChatHistory history,
        string modelId,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        await foreach (var chunk in StreamAsync(history, modelId, cancellationToken))
            sb.Append(chunk);
        return sb.ToString();
    }

    public async Task<List<string>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("ollama");
        var response = await client.GetFromJsonAsync<OllamaTagsResponse>("api/tags", cancellationToken);
        return response?.Models.Select(m => m.Name).ToList() ?? [];
    }

    private static OllamaChatRequest BuildRequest(ChatHistory history, string modelId, bool stream)
    {
        var messages = history
            .Select(m => new OllamaChatMessage(MapRole(m.Role), m.Content ?? string.Empty))
            .ToList();
        return new OllamaChatRequest(modelId, messages, stream);
    }

    private static string MapRole(AuthorRole role)
    {
        if (role == AuthorRole.System) return "system";
        if (role == AuthorRole.Assistant) return "assistant";
        if (role == AuthorRole.Tool) return "tool";
        return "user";
    }

    private static (string? Content, bool Done) ParseChunk(string line)
    {
        using var doc = JsonDocument.Parse(line);
        var root = doc.RootElement;

        var done = root.TryGetProperty("done", out var doneEl) && doneEl.ValueKind == JsonValueKind.True;

        if (!root.TryGetProperty("message", out var message) || message.ValueKind != JsonValueKind.Object)
            return (null, done);

        if (!message.TryGetProperty("content", out var contentEl))
            return (null, done);

        return (ExtractContent(contentEl), done);
    }

    private static string? ExtractContent(JsonElement contentEl)
    {
        switch (contentEl.ValueKind)
        {
            case JsonValueKind.String:
                return contentEl.GetString();

            case JsonValueKind.Object:
                // litert-ai shape: { "content": [{ "text": "...", "type": "text" }], "role": "assistant" }
                if (contentEl.TryGetProperty("content", out var inner))
                    return ExtractContent(inner);
                if (contentEl.TryGetProperty("text", out var textEl) && textEl.ValueKind == JsonValueKind.String)
                    return textEl.GetString();
                return null;

            case JsonValueKind.Array:
                var sb = new StringBuilder();
                foreach (var item in contentEl.EnumerateArray())
                {
                    var part = ExtractContent(item);
                    if (!string.IsNullOrEmpty(part))
                        sb.Append(part);
                }
                return sb.Length > 0 ? sb.ToString() : null;

            default:
                return null;
        }
    }

    private record OllamaChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] List<OllamaChatMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream);

    private record OllamaChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private record OllamaTagsResponse(
        [property: JsonPropertyName("models")] List<OllamaModel> Models);

    private record OllamaModel(
        [property: JsonPropertyName("name")] string Name);
}
