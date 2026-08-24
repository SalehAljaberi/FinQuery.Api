using FinQuery.Api.Models;

namespace FinQuery.Api.Services.Search;

/// <summary>
/// In-memory Inverted Index with Okapi BM25 ranking algorithm.
/// Optimized using SSF (Separate Storage Factor / Chaining) hash tables
/// with pre-sized capacities for minimal re-hashing overhead.
/// </summary>
public class Bm25Index
{
    private readonly ReaderWriterLockSlim _lock = new();

    // Inverted Index: Term -> List of (DocumentId, TermFrequency)
    private Dictionary<string, List<(string DocId, int Frequency)>> _invertedIndex = new(StringComparer.OrdinalIgnoreCase);

    // Document token length: DocId -> Length
    private Dictionary<string, int> _docLengths = new(StringComparer.OrdinalIgnoreCase);

    // Document Metadata: DocId -> (Text, Source, PageNumber)
    private Dictionary<string, (string Text, string Source, int PageNumber)> _docMetadata = new(StringComparer.OrdinalIgnoreCase);

    private double _avgDocLength = 0.0;
    private int _totalDocs = 0;

    // Standard BM25 hyperparameters
    private const double K1 = 1.2;
    private const double B = 0.75;

    public bool IsEmpty => _totalDocs == 0;
    public int DocumentCount => _totalDocs;

    /// <summary>
    /// Rebuilds the inverted index from a collection of document chunks.
    /// </summary>
    public void BuildIndex(IEnumerable<DocumentChunk> chunks)
    {
        var chunkList = chunks.ToList();
        int expectedCount = chunkList.Count;

        // Pre-size hash tables with load factor ~0.72
        int initialCapacity = (int)Math.Max(16, expectedCount / 0.72);

        var newInvertedIndex = new Dictionary<string, List<(string DocId, int Frequency)>>(initialCapacity * 5, StringComparer.OrdinalIgnoreCase);
        var newDocLengths = new Dictionary<string, int>(initialCapacity, StringComparer.OrdinalIgnoreCase);
        var newDocMetadata = new Dictionary<string, (string Text, string Source, int PageNumber)>(initialCapacity, StringComparer.OrdinalIgnoreCase);

        long totalTokens = 0;

        foreach (var chunk in chunkList)
        {
            if (string.IsNullOrWhiteSpace(chunk.Text)) continue;

            var tokens = TextProcessor.Tokenize(chunk.Text, filterStopWords: true);
            int docLength = tokens.Count;

            newDocLengths[chunk.Id] = docLength;
            newDocMetadata[chunk.Id] = (chunk.Text, chunk.Source, chunk.PageNumber);
            totalTokens += docLength;

            // Count term frequencies in this document
            var termFreqMap = new Dictionary<string, int>(tokens.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var token in tokens)
            {
                termFreqMap[token] = termFreqMap.GetValueOrDefault(token, 0) + 1;
            }

            // Update inverted index postings
            foreach (var (term, freq) in termFreqMap)
            {
                if (!newInvertedIndex.TryGetValue(term, out var postings))
                {
                    postings = new List<(string DocId, int Frequency)>();
                    newInvertedIndex[term] = postings;
                }
                postings.Add((chunk.Id, freq));
            }
        }

        _lock.EnterWriteLock();
        try
        {
            _invertedIndex = newInvertedIndex;
            _docLengths = newDocLengths;
            _docMetadata = newDocMetadata;
            _totalDocs = newDocLengths.Count;
            _avgDocLength = _totalDocs > 0 ? (double)totalTokens / _totalDocs : 0.0;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Searches the BM25 inverted index for the best matching documents.
    /// </summary>
    public List<RetrievalResult> Search(string queryText, int topK = 20)
    {
        if (string.IsNullOrWhiteSpace(queryText)) return new List<RetrievalResult>();

        var queryTokens = TextProcessor.Tokenize(queryText, filterStopWords: true);
        if (queryTokens.Count == 0) return new List<RetrievalResult>();

        // Accumulate scores per document: DocId -> BM25 Score
        var docScores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        _lock.EnterReadLock();
        try
        {
            if (_totalDocs == 0 || _avgDocLength == 0) return new List<RetrievalResult>();

            // Remove duplicate query terms for unique IDF calculation
            var uniqueQueryTerms = new HashSet<string>(queryTokens, StringComparer.OrdinalIgnoreCase);

            foreach (var term in uniqueQueryTerms)
            {
                if (!_invertedIndex.TryGetValue(term, out var postings))
                    continue;

                int docFrequency = postings.Count; // n(q)

                // Lucene/Okapi standard Robertson IDF with +1 smoothing
                double idf = Math.Log((_totalDocs - docFrequency + 0.5) / (docFrequency + 0.5) + 1.0);
                if (idf <= 0) idf = 0.0001; // Avoid negative/zero weights

                foreach (var (docId, tf) in postings)
                {
                    int docLen = _docLengths.GetValueOrDefault(docId, 1);
                    double lenNorm = 1.0 - B + B * ((double)docLen / _avgDocLength);

                    // Okapi BM25 TF saturation formula
                    double tfScore = (tf * (K1 + 1.0)) / (tf + K1 * lenNorm);
                    double termScore = idf * tfScore;

                    docScores[docId] = docScores.GetValueOrDefault(docId, 0.0) + termScore;
                }
            }

            if (docScores.Count == 0) return new List<RetrievalResult>();

            // Order by BM25 score descending
            var topMatches = docScores
                .OrderByDescending(kvp => kvp.Value)
                .Take(topK)
                .ToList();

            var results = new List<RetrievalResult>(topMatches.Count);
            foreach (var match in topMatches)
            {
                if (_docMetadata.TryGetValue(match.Key, out var meta))
                {
                    results.Add(new RetrievalResult
                    {
                        Id = match.Key,
                        ChunkText = meta.Text,
                        Source = meta.Source,
                        PageNumber = meta.PageNumber,
                        SimilarityScore = (float)match.Value
                    });
                }
            }

            return results;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
