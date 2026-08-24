using System.Text;
using FinQuery.Api.Models;

namespace FinQuery.Api.Services;

public class PromptService
{
    public string BuildRAGPrompt(string userQuestion, List<RetrievalResult> retrievedChunks)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are FinQuery AI, a local, offline financial intelligence assistant for enterprise financial and ESG analysis.");
        sb.AppendLine();
        sb.AppendLine("BEHAVIOUR RULES:");
        sb.AppendLine("- You MUST only answer using facts found inside the <context> tags below. Period.");
        sb.AppendLine("- Do NOT use any knowledge from your training data. Your training knowledge is completely irrelevant here.");
        sb.AppendLine("- If the question cannot be answered from the <context>, you MUST output exactly: 'This information is not present in the local financial dataset.' — nothing else.");
        sb.AppendLine("- Never hallucinate figures, dates, names, or financial metrics.");
        sb.AppendLine();
        sb.AppendLine("RESPONSE FORMAT (only when context supports the answer):");
        sb.AppendLine("- Answer ONLY the specific question asked. Do not include surrounding context or extra facts.");
        sb.AppendLine("- Source: Document name and page number.");
        sb.AppendLine();
        sb.AppendLine("<context>");

        if (retrievedChunks == null || retrievedChunks.Count == 0)
        {
            sb.AppendLine("[No relevant context found in database]");
        }
        else
        {
            for (int i = 0; i < retrievedChunks.Count; i++)
            {
                var chunk = retrievedChunks[i];
                sb.AppendLine($"<document index=\"{i + 1}\" source=\"{chunk.Source}\" page=\"{chunk.PageNumber}\">");
                sb.AppendLine(chunk.ChunkText);
                sb.AppendLine("</document>");
            }
        }

        sb.AppendLine("</context>");
        sb.AppendLine();
        sb.AppendLine("<question>");
        sb.AppendLine(userQuestion);
        sb.AppendLine("</question>");

        return sb.ToString();
    }
}
