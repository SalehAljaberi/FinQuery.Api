using System.Security.Cryptography;
using System.Text;
using OpenAI.Embeddings;

namespace FinQuery.Api.Services;

public class EmbeddingService
{
    private readonly FoundryLocalService _foundryService;
    private readonly ILogger<EmbeddingService> _logger;
    public const int VectorDimension = 1024; // qwen3-embedding-0.6b uses 1024 dims

    public EmbeddingService(FoundryLocalService foundryService, ILogger<EmbeddingService> logger)
    {
        _foundryService = foundryService;
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new float[VectorDimension];
        }

        try
        {
            var client = await _foundryService.GetEmbeddingClientAsync("qwen3-embedding-0.6b");
            if (client != null)
            {
                var response = await client.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
                if (response?.Value != null)
                {
                    ReadOnlyMemory<float> vectorMemory = response.Value.ToFloats();
                    return vectorMemory.ToArray();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Embedding generation using Foundry Local SDK failed: {Message}. Using fallback deterministic embedding.", ex.Message);
        }

        return GenerateFallbackEmbedding(text);
    }

    public async Task<List<float[]>> GenerateEmbeddingsBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var result = new List<float[]>();
        var textList = texts.ToList();
        
        if (textList.Count == 0) return result;

        try
        {
            var client = await _foundryService.GetEmbeddingClientAsync("qwen3-embedding-0.6b");
            if (client != null)
            {
                // Official batch API — confirmed supported by the direct model client (not the HTTP web service)
                // See: https://learn.microsoft.com/en-us/azure/foundry-local/how-to/how-to-generate-embeddings
                var response = await client.GenerateEmbeddingsAsync(textList, cancellationToken: cancellationToken);
                if (response?.Value != null)
                {
                    foreach (var embedding in response.Value)
                    {
                        result.Add(embedding.ToFloats().ToArray());
                    }
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Batch embedding generation using Foundry Local SDK failed: {Message}. Using fallback deterministic embedding.", ex.Message);
        }

        // Fallback: Generate one by one
        foreach (var text in textList)
        {
            result.Add(GenerateFallbackEmbedding(text));
        }

        return result;
    }

    private static float[] GenerateFallbackEmbedding(string text)
    {
        // Produce a deterministic pseudo-random normalized float[1536] vector based on text hash
        float[] vector = new float[VectorDimension];
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        
        uint seed = BitConverter.ToUInt32(hash, 0);
        Random rng = new Random((int)seed);

        float sumSq = 0f;
        for (int i = 0; i < VectorDimension; i++)
        {
            float val = (float)(rng.NextDouble() * 2.0 - 1.0);
            vector[i] = val;
            sumSq += val * val;
        }

        float norm = MathF.Sqrt(sumSq);
        if (norm > 0f)
        {
            for (int i = 0; i < VectorDimension; i++)
            {
                vector[i] /= norm;
            }
        }

        return vector;
    }
}
