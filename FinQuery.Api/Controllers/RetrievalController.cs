using FinQuery.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinQuery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RetrievalController : ControllerBase
{
    private readonly RetrievalService _retrievalService;
    private readonly ILogger<RetrievalController> _logger;

    public RetrievalController(RetrievalService retrievalService, ILogger<RetrievalController> logger)
    {
        _retrievalService = retrievalService;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/retrieval — Search for relevant document chunks by query.
    /// Body: { "query": "B-BBEE level 2019", "topK": 5, "mode": "csv" }
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Search([FromBody] RetrievalRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Query))
            return BadRequest(new { error = "Query cannot be empty." });

        _logger.LogInformation("Retrieval request: '{Query}' (topK={TopK}, mode={Mode})", 
            request.Query, request.TopK, request.Mode ?? "all");

        var results = await _retrievalService.RetrieveContextAsync(
            request.Query, 
            request.TopK, 
            request.Mode, 
            cancellationToken);

        return Ok(new
        {
            query = request.Query,
            resultsCount = results.Count,
            results
        });
    }
}

/// <summary>
/// Request body for retrieval endpoint. Kept in same file to avoid unnecessary file bloat.
/// </summary>
public class RetrievalRequest
{
    public string Query { get; set; } = string.Empty;
    public int TopK { get; set; } = 5;
    public string? Mode { get; set; }
}
