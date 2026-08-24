using System.Text.Json;
using FinQuery.Api.Models;
using FinQuery.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinQuery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly RetrievalService _retrievalService;
    private readonly ChatCompletionService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        RetrievalService retrievalService,
        ChatCompletionService chatService,
        ILogger<ChatController> logger)
    {
        _retrievalService = retrievalService;
        _chatService = chatService;
        _logger = logger;
    }

    [HttpPost]
    public async Task Chat([FromBody] ChatRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request?.Question))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync("Question cannot be empty.", cancellationToken);
            return;
        }

        string mode = string.Equals(request.Mode, "pdf", StringComparison.OrdinalIgnoreCase) ? "pdf" : "csv";

        // Retrieve top-5 relevant chunks
        var retrievedChunks = await _retrievalService.RetrieveContextAsync(request.Question, topK: 3, mode: mode, cancellationToken: cancellationToken);

        Response.ContentType = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";

        // ── Relevance Gate ────────────────────────────────────────────────────────
        // If no chunks passed the MinRelevanceThreshold in RetrievalService,
        // the question is out-of-domain. Reject immediately without calling the LLM.
        if (retrievedChunks.Count == 0)
        {
            _logger.LogInformation("No relevant chunks found for question '{Question}'. Returning OOD rejection.", request.Question);
            var emptySourcesEvent = JsonSerializer.Serialize(new { type = "sources", sources = retrievedChunks });
            await Response.WriteAsync($"data: {emptySourcesEvent}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            const string rejectionMsg = "This information is not present in the local financial dataset.";
            var tokenEvent = JsonSerializer.Serialize(new { type = "token", token = rejectionMsg });
            await Response.WriteAsync($"data: {tokenEvent}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            return;
        }
        // ─────────────────────────────────────────────────────────────────────────

        // Send sources metadata event first
        var sourcesEventData = JsonSerializer.Serialize(new { type = "sources", sources = retrievedChunks });
        await Response.WriteAsync($"data: {sourcesEventData}\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);

        // Stream answer tokens via SSE
        await foreach (var token in _chatService.StreamChatResponseAsync(request.Question, retrievedChunks, request.ConversationHistory, cancellationToken))
        {
            var tokenEventData = JsonSerializer.Serialize(new { type = "token", token = token });
            await Response.WriteAsync($"data: {tokenEventData}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        // Send done event
        await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
