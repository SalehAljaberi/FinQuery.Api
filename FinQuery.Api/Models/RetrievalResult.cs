namespace FinQuery.Api.Models;

public class RetrievalResult
{
    public string Id { get; set; } = string.Empty;
    public string ChunkText { get; set; } = string.Empty;
    /// <summary>RRF-fused score (used for ranking only). Always in ~0.008–0.033 range — NOT a relevance indicator.</summary>
    public float SimilarityScore { get; set; }
    /// <summary>Raw cosine similarity from pgvector (0–1). Used as the true out-of-domain gate.</summary>
    public float CosineScore { get; set; }
    public string Source { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
}
