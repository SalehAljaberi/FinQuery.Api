namespace FinQuery.Api.Models;

public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<RetrievalResult> Sources { get; set; } = new();
}
