using Ai.WebUI.Services.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;

namespace Ai.WebUI.Tests.Services.AI;

[TestClass]
public class ChatHistoryReducerTests
{
    private static IConfiguration BuildConfig(int threshold = 100) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ChatHistory:ReduceThreshold"] = threshold.ToString(),
                ["Ollama:DefaultModel"] = "llama3.2"
            })
            .Build();

    private static ChatHistory BuildHistory(int messageCount, int charsPerMessage = 10)
    {
        var h = new ChatHistory();
        h.AddSystemMessage("You are a helpful assistant.");
        for (var i = 0; i < messageCount; i++)
        {
            if (i % 2 == 0) h.AddUserMessage(new string('u', charsPerMessage));
            else h.AddAssistantMessage(new string('a', charsPerMessage));
        }
        return h;
    }

    [TestMethod]
    public async Task ReduceAsync_WhenUnderThreshold_ReturnsNull()
    {
        var mockChat = new Mock<IOllamaChatService>();
        var reducer = new ChatHistoryReducer(mockChat.Object, BuildConfig(threshold: 10000));
        var history = BuildHistory(messageCount: 3, charsPerMessage: 10);

        var result = await reducer.ReduceAsync(history);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ReduceAsync_WhenOverThreshold_ReturnsShorterHistory()
    {
        var mockChat = new Mock<IOllamaChatService>();
        mockChat
            .Setup(s => s.CompleteAsync(It.IsAny<ChatHistory>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Summary of earlier messages.");

        var reducer = new ChatHistoryReducer(mockChat.Object, BuildConfig(threshold: 1));
        var history = BuildHistory(messageCount: 20, charsPerMessage: 50);

        var result = await reducer.ReduceAsync(history);

        Assert.IsNotNull(result);
        var list = result.ToList();
        Assert.IsTrue(list.Count < history.Count);
        Assert.IsTrue(list.Any(m => m.Content!.Contains("[Summary")));
    }

    [TestMethod]
    public async Task ReduceAsync_WhenOverThreshold_PreservesSystemMessage()
    {
        var mockChat = new Mock<IOllamaChatService>();
        mockChat
            .Setup(s => s.CompleteAsync(It.IsAny<ChatHistory>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Brief summary.");

        var reducer = new ChatHistoryReducer(mockChat.Object, BuildConfig(threshold: 1));
        var history = BuildHistory(messageCount: 20, charsPerMessage: 50);

        var result = await reducer.ReduceAsync(history);

        Assert.IsNotNull(result);
        Assert.AreEqual(AuthorRole.System, result.First().Role);
    }
}
