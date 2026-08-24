namespace FinQuery.Api.Models;

public class ChatRequest
{
    public string Question { get; set; } = string.Empty;
    public string Mode { get; set; } = "csv"; // "csv" or "pdf"
    public List<ChatMessageHistory>? ConversationHistory { get; set; }
}

public class ChatMessageHistory
{
    public string Role { get; set; } = "user"; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
}
