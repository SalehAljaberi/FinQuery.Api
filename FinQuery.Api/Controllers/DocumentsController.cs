using FinQuery.Api.Data;
using FinQuery.Api.Models;
using FinQuery.Api.Services.Ingestion;
using FinQuery.Api.Services.Search;
using Microsoft.AspNetCore.Mvc;

namespace FinQuery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly PostgresVectorStore _vectorStore;
    private readonly PdfVisionIngestionService _pdfService;
    private readonly Bm25Index _bm25Index;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        PostgresVectorStore vectorStore,
        PdfVisionIngestionService pdfService,
        Bm25Index bm25Index,
        ILogger<DocumentsController> logger)
    {
        _vectorStore = vectorStore;
        _pdfService = pdfService;
        _bm25Index = bm25Index;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetDocuments([FromQuery] string? mode = null)
    {
        var docs = await _vectorStore.GetDocumentsAsync(mode);
        return Ok(docs);
    }

    [HttpDelete("{filename}")]
    public async Task<IActionResult> DeleteDocument(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return BadRequest("Filename is required.");
        }

        string decodedFilename = Uri.UnescapeDataString(filename);
        int deleted = await _vectorStore.DeleteDocumentAsync(decodedFilename);

        // Rebuild BM25 index after deletion
        var remaining = await _vectorStore.GetAllChunksAsync();
        _bm25Index.BuildIndex(remaining);

        return Ok(new
        {
            Success = true,
            DeletedChunks = deleted,
            Source = decodedFilename
        });
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile file, [FromServices] CsvIngestionService csvService, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Success = false, Message = "No file was uploaded." });
        }

        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".pdf" && ext != ".csv")
        {
            return BadRequest(new { Success = false, Message = "Only PDF and CSV files are supported." });
        }

        string targetFolder = Path.Combine(Directory.GetCurrentDirectory(), "Docs", "Structured-data");
        Directory.CreateDirectory(targetFolder);

        string safeFileName = Path.GetFileName(file.FileName);
        string targetPath = Path.Combine(targetFolder, safeFileName);

        _logger.LogInformation("Saving uploaded PDF {FileName} ({Bytes} bytes) to {Path}", safeFileName, file.Length, targetPath);
        using (var stream = new FileStream(targetPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        // Ingest the newly uploaded Document
        if (ext == ".csv")
        {
            var result = await csvService.IngestCsvAsync(targetPath, safeFileName, cancellationToken: cancellationToken);
            return Ok(result);
        }
        else
        {
            var result = await _pdfService.IngestPdfFolderAsync(
                pdfFolderPath: targetFolder,
                fileFilter: safeFileName,
                cancellationToken: cancellationToken);
            
            return Ok(result);
        }
    }
}
