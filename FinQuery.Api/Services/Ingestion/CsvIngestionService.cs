using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FinQuery.Api.Data;
using FinQuery.Api.Models;

namespace FinQuery.Api.Services.Ingestion;

public class CsvIngestionService
{
    private readonly PostgresVectorStore _vectorStore;
    private readonly EmbeddingService _embeddingService;
    private readonly ILogger<CsvIngestionService> _logger;

    public CsvIngestionService(
        PostgresVectorStore vectorStore,
        EmbeddingService embeddingService,
        ILogger<CsvIngestionService> logger)
    {
        _vectorStore = vectorStore;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<IngestionResponse> IngestCsvAsync(string csvPath, string sourceName, bool resume = true, int batchSize = 4, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        if (!File.Exists(csvPath))
        {
            return new IngestionResponse
            {
                Success = false,
                Message = $"CSV file not found at path: {csvPath}",
                Mode = "csv"
            };
        }

        _logger.LogInformation("Starting CSV Ingestion from {CsvPath} (Source: {SourceName})...", csvPath, sourceName);

        // Always start fresh for this specific CSV source to prevent duplicate chunks
        await _vectorStore.DeleteDocumentAsync(sourceName);

        var seenHashes = new HashSet<int>();
        var batchTexts = new List<string>();
        int currentRow = 0;
        int totalSaved = 0;

        // Pre-count total rows for progress bar
        int totalRows = 0;
        using (var lineCounter = new StreamReader(csvPath))
        {
            while (await lineCounter.ReadLineAsync() != null) totalRows++;
        }
        totalRows -= 1; // subtract header

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) 
        { 
            HasHeaderRecord = true, 
            MissingFieldFound = null, 
            HeaderValidated = null, 
            BadDataFound = null 
        });
        
        await csv.ReadAsync();
        csv.ReadHeader();

        _logger.LogInformation("Starting sequential ingestion. Batch size: {BatchSize}", batchSize);
        Console.WriteLine(); // Blank line for the progress bar to overwrite

        var stopwatch = Stopwatch.StartNew();

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            currentRow++;

            try
            {
                string context = (csv.GetField<string>(2) ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(context) || !seenHashes.Add(context.GetHashCode()))
                    continue;

                batchTexts.Add(context);

                if (batchTexts.Count >= batchSize)
                {
                    var textsToProcess = batchTexts.ToList();
                    batchTexts.Clear();
                    int rowCheckpoint = currentRow;
                    
                    try
                    {
                        var embeddings = await _embeddingService.GenerateEmbeddingsBatchAsync(textsToProcess, cancellationToken);
                        var chunks = new List<DocumentChunk>();
                        for (int i = 0; i < textsToProcess.Count; i++)
                        {
                            chunks.Add(new DocumentChunk
                            {
                                Id = Guid.NewGuid().ToString(),
                                Text = textsToProcess[i],
                                Embedding = embeddings[i],
                                Source = sourceName,
                                PageNumber = 1
                            });
                        }
                        await _vectorStore.SaveChunksAsync(chunks, "csv");
                        totalSaved += chunks.Count;
                        
                        // In-place console loading bar
                        if (totalSaved % 20 == 0 || rowCheckpoint == totalRows)
                        {
                            double percent = (double)rowCheckpoint / totalRows * 100;
                            int bars = (int)(percent / 5); // 20 blocks total
                            string barStr = new string('█', bars).PadRight(20, '-');
                            
                            double elapsedSec = stopwatch.Elapsed.TotalSeconds;
                            double speed = elapsedSec > 0 ? totalSaved / elapsedSec : 0;
                            long ramMb = GC.GetTotalMemory(false) / 1024 / 1024;

                            Console.Write($"\r[{barStr}] {percent:F1}% ({rowCheckpoint} / {totalRows}) | Saved: {totalSaved} | {speed:F1} chunks/sec | RAM: {ramMb} MB    ");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "\nFailed to process batch ending at row {Row}", rowCheckpoint);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("\nCSV read error at row {Row}: {Message}", currentRow, ex.Message);
            }
        }

        // Process any remaining texts in the final batch
        if (batchTexts.Count > 0)
        {
            var embeddings = await _embeddingService.GenerateEmbeddingsBatchAsync(batchTexts, cancellationToken);
            var chunks = new List<DocumentChunk>();
            for (int i = 0; i < batchTexts.Count; i++)
            {
                chunks.Add(new DocumentChunk
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = batchTexts[i],
                    Embedding = embeddings[i],
                    Source = sourceName,
                    PageNumber = 1
                });
            }
            await _vectorStore.SaveChunksAsync(chunks, "csv");
            totalSaved += chunks.Count;
            Console.WriteLine(); // Break out of the \r line
            _logger.LogInformation("Processed final row {Row}. Total unique saved: {TotalSaved}", currentRow, totalSaved);
        }

        return new IngestionResponse
        {
            Success = true,
            Message = $"CSV Ingestion completed successfully. Total unique contexts saved this run: {totalSaved}",
            Mode = "csv"
        };
    }
}
