using FinQuery.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace FinQuery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly PostgresVectorStore _vectorStore;

    public HealthController(PostgresVectorStore vectorStore)
    {
        _vectorStore = vectorStore;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        int totalChunks = await _vectorStore.GetChunkCountAsync();
        return Ok(new
        {
            Status = "Healthy",
            Service = "FinQuery AI API",
            Timestamp = DateTime.UtcNow,
            TotalIndexedChunks = totalChunks
        });
    }
}
