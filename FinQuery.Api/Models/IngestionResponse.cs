namespace FinQuery.Api.Models;

public class IngestionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ChunksProcessed { get; set; }
    public string Mode { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
}
