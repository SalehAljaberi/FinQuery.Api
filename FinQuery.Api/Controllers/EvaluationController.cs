using FinQuery.Api.Services.Evaluation;
using Microsoft.AspNetCore.Mvc;

namespace FinQuery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EvaluationController : ControllerBase
{
    private readonly HitRateEvaluator _evaluator;

    public EvaluationController(HitRateEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    [HttpPost]
    public async Task<ActionResult<EvaluationReport>> Evaluate([FromQuery] int maxSamples = 50, CancellationToken cancellationToken = default)
    {
        var report = await _evaluator.EvaluateAsync(maxSamples: maxSamples, cancellationToken: cancellationToken);
        return Ok(report);
    }
}
