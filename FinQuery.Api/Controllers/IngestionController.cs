using FinQuery.Api.Data;
using FinQuery.Api.Models;
using FinQuery.Api.Services.Ingestion;
using Microsoft.AspNetCore.Mvc;

namespace FinQuery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngestionController : ControllerBase
{
    private readonly CsvIngestionService _csvService;
    private readonly PdfVisionIngestionService _pdfService;
    private readonly PostgresVectorStore _vectorStore;
    private readonly ILogger<IngestionController> _logger;

    public IngestionController(
        CsvIngestionService csvService,
        PdfVisionIngestionService pdfService,
        PostgresVectorStore vectorStore,
        ILogger<IngestionController> logger)
    {
        _csvService = csvService;
        _pdfService = pdfService;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<IngestionResponse>> Ingest(
        [FromQuery] string mode = "csv",
        [FromQuery] string? file = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received ingestion request for mode: {Mode}, fileFilter: {File}", mode, file ?? "ALL");

        if (mode.Equals("pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdfResponse = await _pdfService.IngestPdfFolderAsync(
                fileFilter: file,
                cancellationToken: cancellationToken);
            return Ok(pdfResponse);
        }
        else
        {
            var csvResponse = await _csvService.IngestCsvAsync("Ingestion/Data_ret.csv", "Data_ret.csv", cancellationToken: cancellationToken);
            return Ok(csvResponse);
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        int csvCount = await _vectorStore.GetChunkCountAsync("csv");
        int pdfCount = await _vectorStore.GetChunkCountAsync("pdf");
        int totalCount = await _vectorStore.GetChunkCountAsync(null);

        return Ok(new
        {
            CsvChunks = csvCount,
            PdfChunks = pdfCount,
            TotalChunks = totalCount
        });
    }
}
