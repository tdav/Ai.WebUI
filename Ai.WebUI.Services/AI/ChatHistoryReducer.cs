using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ai.WebUI.Services.AI;

public class ChatHistoryReducer(IOllamaChatService chatService, IConfiguration configuration)
    : IHistoryReducer
{
    private readonly int reduceThreshold =
        configuration.GetValue<int>("ChatHistory:ReduceThreshold", 3000);
    private readonly string defaultModel =
        configuration["Ollama:DefaultModel"] ?? "llama3.2";
    private const int KeepLastMessages = 6;

    public async Task<IEnumerable<ChatMessageContent>?> ReduceAsync(
        IReadOnlyList<ChatMessageContent> chatHistory,
        CancellationToken cancellationToken = default)
    {
        if (EstimateTokens(chatHistory) < reduceThreshold)
            return null;

        var systemMessages = chatHistory.Where(m => m.Role == AuthorRole.System).ToList();
        var nonSystem = chatHistory.Where(m => m.Role != AuthorRole.System).ToList();

        if (nonSystem.Count <= KeepLastMessages)
            return null;

        var toSummarize = nonSystem.Take(nonSystem.Count - KeepLastMessages).ToList();
        var toKeep = nonSystem.TakeLast(KeepLastMessages).ToList();

        var summaryRequest = new ChatHistory();
        summaryRequest.AddUserMessage(
            "Briefly summarize the following conversation in 2-3 sentences:\n" +
            string.Join("\n", toSummarize.Select(m => $"{m.Role.Label}: {m.Content}")));

        var summary = await chatService.CompleteAsync(summaryRequest, defaultModel, cancellationToken);

        var reduced = new List<ChatMessageContent>();
        reduced.AddRange(systemMessages);
        reduced.Add(new ChatMessageContent(AuthorRole.Assistant,
            $"[Summary of earlier conversation] {summary}"));
        reduced.AddRange(toKeep);

        return reduced;
    }

    private static int EstimateTokens(IReadOnlyList<ChatMessageContent> history) =>
        history.Sum(m => (m.Content?.Length ?? 0) / 4);
}
