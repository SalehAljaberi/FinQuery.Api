using System.Runtime.CompilerServices;
using FinQuery.Api.Models;
using OpenAI.Chat;

namespace FinQuery.Api.Services;

public class ChatCompletionService
{
    private readonly FoundryLocalService _foundryService;
    private readonly PromptService _promptService;
    private readonly ILogger<ChatCompletionService> _logger;

    public ChatCompletionService(
        FoundryLocalService foundryService,
        PromptService promptService,
        ILogger<ChatCompletionService> logger)
    {
        _foundryService = foundryService;
        _promptService = promptService;
        _logger = logger;
    }

    public async IAsyncEnumerable<string> StreamChatResponseAsync(
        string question,
        List<RetrievalResult> retrievedChunks,
        List<ChatMessageHistory>? conversationHistory = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string fullPrompt = _promptService.BuildRAGPrompt(question, retrievedChunks);

        ChatClient? client = null;
        try
        {
            client = await _foundryService.GetChatClientAsync("qwen2.5-0.5b");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not acquire ChatClient from Foundry Local. Using synthesis fallback.");
        }

        if (client != null)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert financial extraction AI. You MUST extract answers ONLY from the <context> provided. Do not use outside knowledge. If the context does not contain the answer, you MUST say: 'This information is not present in the local financial dataset.'")
            };

            // Add previous conversational turns (up to last 6 messages to stay within context limits)
            if (conversationHistory != null && conversationHistory.Count > 0)
            {
                var recentHistory = conversationHistory.TakeLast(6);
                foreach (var hist in recentHistory)
                {
                    if (string.Equals(hist.Role, "assistant", StringComparison.OrdinalIgnoreCase))
                    {
                        messages.Add(new AssistantChatMessage(hist.Content));
                    }
                    else if (string.Equals(hist.Role, "user", StringComparison.OrdinalIgnoreCase))
                    {
                        messages.Add(new UserChatMessage(hist.Content));
                    }
                }
            }

            messages.Add(new UserChatMessage(fullPrompt));

            var updates = client.CompleteChatStreamingAsync(messages, cancellationToken: cancellationToken);
            await foreach (var update in updates)
            {
                foreach (var textPart in update.ContentUpdate)
                {
                    if (!string.IsNullOrEmpty(textPart.Text))
                    {
                        yield return textPart.Text;
                    }
                }
            }
            yield break;
        }

        // Fallback RAG response generator if local LLM is still downloading or offline
        string fallbackAnswer = GenerateFallbackRAGAnswer(question, retrievedChunks);
        string[] tokens = fallbackAnswer.Split(' ');
        foreach (var token in tokens)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return token + " ";
            await Task.Delay(25, cancellationToken);
        }
    }

    private static string GenerateFallbackRAGAnswer(string question, List<RetrievalResult> retrievedChunks)
    {
        if (retrievedChunks == null || retrievedChunks.Count == 0)
        {
            return "This information is not present in the local financial dataset.";
        }

        var topChunk = retrievedChunks[0];
        return $"Based on the financial records in [{topChunk.Source}] (Confidence: {topChunk.SimilarityScore:P1}):\n\n{topChunk.ChunkText}";
    }
}
