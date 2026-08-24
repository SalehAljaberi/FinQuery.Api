using FinQuery.Api.Data;
using FinQuery.Api.Models;
using FinQuery.Api.Services.Search;

namespace FinQuery.Api.Services;

public class RetrievalService
{
    private readonly PostgresVectorStore _vectorStore;
    private readonly EmbeddingService _embeddingService;
    private readonly Bm25Index _bm25Index;
    private readonly ILogger<RetrievalService> _logger;

    public RetrievalService(
        PostgresVectorStore vectorStore,
        EmbeddingService embeddingService,
        Bm25Index bm25Index,
        ILogger<RetrievalService> logger)
    {
        _vectorStore = vectorStore;
        _embeddingService = embeddingService;
        _bm25Index = bm25Index;
        _logger = logger;
    }

    public async Task<List<RetrievalResult>> RetrieveContextAsync(string userQuestion, int topK = 3, string? mode = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            return new List<RetrievalResult>();
        }

        // 1. Ensure BM25 in-memory index is populated
        await EnsureBm25IndexLoadedAsync(mode);

        // 2. Dense Semantic Search (via pgvector)
        _logger.LogInformation("Generating embedding for user query: '{Question}' (mode: {Mode})...", userQuestion, mode ?? "all");
        float[] queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(userQuestion, cancellationToken);
        var vectorResults = await _vectorStore.SearchVectorAsync(queryEmbedding, topK: 20, mode: mode);

        // 3. Sparse Keyword Search (via In-Memory BM25 with Stop Words & Regex Tokenization)
        var bm25Results = _bm25Index.Search(userQuestion, topK: 20);

        // 4. Reciprocal Rank Fusion (RRF)
        // NOTE: RRF scores are purely rank-based (always ~0.008–0.033) and are NOT
        // a semantic relevance measure. They are used ONLY for ordering results.
        // The actual out-of-domain gate uses the raw cosine similarity from pgvector.
        const double k = 60.0;
        var rrfScores = new Dictionary<string, (double score, RetrievalResult result)>();

        // Add semantic ranks — preserve the raw CosineScore from pgvector
        for (int i = 0; i < vectorResults.Count; i++)
        {
            var item = vectorResults[i];
            int rank = i + 1;
            double score = 1.0 / (k + rank);

            if (rrfScores.TryGetValue(item.Id, out var existing))
            {
                rrfScores[item.Id] = (existing.score + score, existing.result);
            }
            else
            {
                rrfScores[item.Id] = (score, item); // item.CosineScore is set from pgvector
            }
        }

        // Add BM25 ranks
        for (int i = 0; i < bm25Results.Count; i++)
        {
            var item = bm25Results[i];
            int rank = i + 1;
            double score = 1.0 / (k + rank);

            if (rrfScores.TryGetValue(item.Id, out var existing))
            {
                rrfScores[item.Id] = (existing.score + score, existing.result);
            }
            else
            {
                rrfScores[item.Id] = (score, item);
            }
        }

        // Sort by merged RRF score (for ranking) and preserve CosineScore for the gate check
        var orderedResults = rrfScores.Values
            .OrderByDescending(x => x.score)
            .Take(topK)
            .Select(x => new RetrievalResult
            {
                Id = x.result.Id,
                ChunkText = x.result.ChunkText,
                Source = x.result.Source,
                PageNumber = x.result.PageNumber,
                SimilarityScore = (float)x.score,
                CosineScore = x.result.CosineScore  // raw 0-1 cosine from pgvector
            })
            .ToList();

        // ── Out-of-Domain (OOD) Gate ─────────────────────────────────────────────
        // Use the TOP result's raw cosine similarity as a true semantic relevance signal.
        // Cosine similarity of 0–1: values < 0.50 mean the query embedding is semantically
        // distant from anything in our corpus — the question is likely off-domain.
        // Threshold calibration:
        //   - "Pick n Pay turnover FY23"  → top cosine ~0.65-0.80 ✓
        //   - "How to make cheesecake"    → top cosine ~0.30-0.45 ✗ (blocked)
        const float CosineThreshold = 0.50f;
        float topCosine = orderedResults.Count > 0 ? orderedResults[0].CosineScore : 0f;

        if (topCosine < CosineThreshold)
        {
            _logger.LogInformation(
                "OOD gate triggered: top cosine similarity {TopCosine:F3} < {Threshold:F3}. Returning empty (question is off-domain).",
                topCosine, CosineThreshold);
            return new List<RetrievalResult>();
        }

        _logger.LogInformation(
            "Hybrid Retrieval: {Count} chunks returned. Top cosine: {TopCosine:F3} (gate: {Threshold:F3}), Vector candidates: {VCount}, BM25 candidates: {BCount}.",
            orderedResults.Count, topCosine, CosineThreshold, vectorResults.Count, bm25Results.Count);

        return orderedResults;
    }

    private async Task EnsureBm25IndexLoadedAsync(string? mode)
    {
        if (_bm25Index.IsEmpty)
        {
            _logger.LogInformation("BM25 index is empty. Loading document chunks from PostgreSQL...");
            var chunks = await _vectorStore.GetAllChunksAsync(mode);
            _bm25Index.BuildIndex(chunks);
            _logger.LogInformation("Built BM25 index with {Count} document chunks.", chunks.Count);
        }
    }
}
