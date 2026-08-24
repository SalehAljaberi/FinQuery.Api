using FinQuery.Api.Data;
using FinQuery.Api.Models;
using FinQuery.Api.Services.Search;
using UglyToad.PdfPig;

namespace FinQuery.Api.Services.Ingestion;

public class PdfVisionIngestionService
{
    private readonly PostgresVectorStore _vectorStore;
    private readonly EmbeddingService _embeddingService;
    private readonly Bm25Index _bm25Index;
    private readonly SlidingWindowChunker _chunker;
    private readonly ILogger<PdfVisionIngestionService> _logger;

    public PdfVisionIngestionService(
        PostgresVectorStore vectorStore,
        EmbeddingService embeddingService,
        Bm25Index bm25Index,
        ILogger<PdfVisionIngestionService> logger)
    {
        _vectorStore = vectorStore;
        _embeddingService = embeddingService;
        _bm25Index = bm25Index;
        _chunker = new SlidingWindowChunker(chunkSize: 1000, overlap: 200);
        _logger = logger;
    }

    public async Task<IngestionResponse> IngestPdfFolderAsync(
        string pdfFolderPath = "Docs/Structured-data",
        string? fileFilter = null,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        if (!Directory.Exists(pdfFolderPath))
        {
            return new IngestionResponse
            {
                Success = false,
                Message = $"PDF directory not found at path: {pdfFolderPath}",
                Mode = "pdf"
            };
        }

        string[] allPdfFiles = Directory.GetFiles(pdfFolderPath, "*.pdf", SearchOption.AllDirectories);
        string[] pdfFiles = string.IsNullOrWhiteSpace(fileFilter)
            ? allPdfFiles
            : allPdfFiles.Where(f => Path.GetFileName(f).Contains(fileFilter, StringComparison.OrdinalIgnoreCase)).ToArray();

        if (pdfFiles.Length == 0)
        {
            return new IngestionResponse
            {
                Success = false,
                Message = $"No PDF files matching filter '{fileFilter ?? "*"}' found in directory: {pdfFolderPath}",
                Mode = "pdf"
            };
        }

        _logger.LogInformation("Starting Native PDF Ingestion on {Count} file(s) from {Folder} (Filter: {Filter})...",
            pdfFiles.Length, pdfFolderPath, fileFilter ?? "ALL");

        // Clear existing PDF chunks
        await _vectorStore.ClearChunksAsync("pdf");

        int totalChunksProcessed = 0;
        var allIngestedChunks = new List<DocumentChunk>();

        foreach (var pdfPath in pdfFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string fileName = Path.GetFileName(pdfPath);
            _logger.LogInformation("Processing PDF file: {FileName}...", fileName);

            try
            {
                using var document = PdfDocument.Open(pdfPath);
                int pageCount = document.NumberOfPages;
                _logger.LogInformation("File {FileName} has {PageCount} pages.", fileName, pageCount);

                for (int pageIndex = 1; pageIndex <= pageCount; pageIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var page = document.GetPage(pageIndex);
                    
                    // 1. Extract raw text from page
                    string rawPageText = page.Text;

                    if (!string.IsNullOrWhiteSpace(rawPageText))
                    {
                        // 2. Clean table/concatenated text using regex
                        string cleanedText = TextProcessor.CleanPdfTableText(rawPageText);

                        // 3. Chunk the text
                        var chunks = _chunker.ChunkText(cleanedText, fileName, pageIndex);
                        
                        // 4. Generate embeddings
                        foreach (var chunk in chunks)
                        {
                            chunk.Embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Text, cancellationToken);
                        }

                        // 5. Save chunks to Postgres
                        await _vectorStore.SaveChunksAsync(chunks, "pdf");
                        allIngestedChunks.AddRange(chunks);
                        totalChunksProcessed += chunks.Count;
                        
                        if (pageIndex % 10 == 0 || pageIndex == pageCount)
                        {
                            _logger.LogInformation("  [{FileName}] Processed page {Page}/{Total}. Total chunks so far: {TotalChunks}",
                                fileName, pageIndex, pageCount, totalChunksProcessed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PDF file {FileName}.", fileName);
            }
        }

        // 6. Build in-memory BM25 index with all ingested chunks
        _logger.LogInformation("Building BM25 index with {Count} chunks...", allIngestedChunks.Count);
        _bm25Index.BuildIndex(allIngestedChunks);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogInformation("Completed Native PDF Ingestion of {Count} chunks from {FileCount} file(s) in {Duration}.",
            totalChunksProcessed, pdfFiles.Length, duration);

        return new IngestionResponse
        {
            Success = true,
            Message = $"Successfully ingested {totalChunksProcessed} chunks from {pdfFiles.Length} PDF file(s) via Native text pipeline with BM25 indexing.",
            ChunksProcessed = totalChunksProcessed,
            Mode = "pdf",
            Duration = duration
        };
    }
}
