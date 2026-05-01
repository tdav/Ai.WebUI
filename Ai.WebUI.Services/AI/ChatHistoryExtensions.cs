using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ai.WebUI.Services.AI;

public record ChatMessageDto(string Role, string Content);

public static class ChatHistoryExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static string ToJson(this ChatHistory history) =>
        JsonSerializer.Serialize(
            history.Select(m => new ChatMessageDto(m.Role.Label ?? string.Empty, m.Content ?? string.Empty)));

    public static ChatHistory ToChatHistory(this string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new ChatHistory();
        var dtos = JsonSerializer.Deserialize<List<ChatMessageDto>>(json, JsonOptions) ?? [];
        var history = new ChatHistory();
        foreach (var dto in dtos)
        {
            switch (dto.Role.ToLowerInvariant())
            {
                case "user": history.AddUserMessage(dto.Content); break;
                case "assistant": history.AddAssistantMessage(dto.Content); break;
                case "system": history.AddSystemMessage(dto.Content); break;
                default: break;
            }
        }
        return history;
    }

    public static int EstimateTokens(this ChatHistory history) =>
        history.Sum(m => (m.Content?.Length ?? 0) / 4);
}
