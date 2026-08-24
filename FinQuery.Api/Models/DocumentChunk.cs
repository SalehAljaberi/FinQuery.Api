namespace FinQuery.Api.Models;

public class DocumentChunk
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public string Source { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
}
