using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FinQuery.Api.Models;

namespace FinQuery.Api.Services.Evaluation;

public class EvaluationReport
{
    public int TotalEvaluated { get; set; }
    public double HitRateAt3 { get; set; }
    public double HitRateAt5 { get; set; }
    public double MeanReciprocalRank { get; set; }
    public TimeSpan TotalTime { get; set; }
}

public class HitRateEvaluator
{
    private readonly RetrievalService _retrievalService;
    private readonly ILogger<HitRateEvaluator> _logger;

    public HitRateEvaluator(RetrievalService retrievalService, ILogger<HitRateEvaluator> logger)
    {
        _retrievalService = retrievalService;
        _logger = logger;
    }

    public async Task<EvaluationReport> EvaluateAsync(string csvPath = "Ingestion/Data_ret.csv", int maxSamples = 100, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        int hitsAt3 = 0;
        int hitsAt5 = 0;
        double mrrSum = 0;

        var testCases = new List<(string Question, string GroundTruthContext)>();

        if (File.Exists(csvPath))
        {
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
            while (await csv.ReadAsync() && testCases.Count < maxSamples)
            {
                string question = csv.GetField<string>(1) ?? string.Empty;
                string context = csv.GetField<string>(2) ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(question) && !string.IsNullOrWhiteSpace(context))
                {
                    testCases.Add((question.Trim(), context.Trim()));
                }
            }
        }

        _logger.LogInformation("Running Hit Rate evaluation on {Count} test cases...", testCases.Count);

        for (int i = 0; i < testCases.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var testCase = testCases[i];

            var retrieved = await _retrievalService.RetrieveContextAsync(testCase.Question, topK: 5, mode: "csv", cancellationToken: cancellationToken);

            int rank = -1;
            for (int r = 0; r < retrieved.Count; r++)
            {
                if (IsMatchingContext(retrieved[r].ChunkText, testCase.GroundTruthContext))
                {
                    rank = r + 1;
                    break;
                }
            }

            if (rank > 0)
            {
                if (rank <= 3) hitsAt3++;
                if (rank <= 5) hitsAt5++;
                mrrSum += 1.0 / rank;
            }
        }

        int total = testCases.Count > 0 ? testCases.Count : 1;
        var report = new EvaluationReport
        {
            TotalEvaluated = testCases.Count,
            HitRateAt3 = (double)hitsAt3 / total,
            HitRateAt5 = (double)hitsAt5 / total,
            MeanReciprocalRank = mrrSum / total,
            TotalTime = DateTime.UtcNow - startTime
        };

        _logger.LogInformation("Evaluation complete: Hit@3={HitRateAt3:P1}, Hit@5={HitRateAt5:P1}, MRR={MRR:F3}", report.HitRateAt3, report.HitRateAt5, report.MeanReciprocalRank);

        return report;
    }

    private static bool IsMatchingContext(string retrieved, string groundTruth)
    {
        if (string.IsNullOrWhiteSpace(retrieved) || string.IsNullOrWhiteSpace(groundTruth)) return false;
        return retrieved.Contains(groundTruth, StringComparison.OrdinalIgnoreCase) ||
               groundTruth.Contains(retrieved, StringComparison.OrdinalIgnoreCase);
    }
}
