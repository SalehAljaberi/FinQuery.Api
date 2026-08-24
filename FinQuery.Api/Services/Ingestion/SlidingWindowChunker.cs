using FinQuery.Api.Models;

namespace FinQuery.Api.Services.Ingestion;

public class SlidingWindowChunker
{
    private readonly int _chunkSize;
    private readonly int _overlap;

    public SlidingWindowChunker(int chunkSize = 1000, int overlap = 200)
    {
        _chunkSize = chunkSize;
        _overlap = overlap;
    }

    public List<DocumentChunk> ChunkText(string content, string sourceName, int pageNumber = 1)
    {
        var chunks = new List<DocumentChunk>();
        if (string.IsNullOrWhiteSpace(content)) return chunks;

        string cleanText = content.Trim();
        if (cleanText.Length <= _chunkSize)
        {
            chunks.Add(new DocumentChunk
            {
                Text = cleanText,
                Source = sourceName,
                PageNumber = pageNumber
            });
            return chunks;
        }

        int step = _chunkSize - _overlap;
        if (step <= 0) step = _chunkSize / 2;

        for (int i = 0; i < cleanText.Length; i += step)
        {
            int length = Math.Min(_chunkSize, cleanText.Length - i);
            string chunkText = cleanText.Substring(i, length).Trim();

            if (!string.IsNullOrWhiteSpace(chunkText))
            {
                chunks.Add(new DocumentChunk
                {
                    Text = chunkText,
                    Source = sourceName,
                    PageNumber = pageNumber
                });
            }

            if (i + length >= cleanText.Length) break;
        }

        return chunks;
    }
}
