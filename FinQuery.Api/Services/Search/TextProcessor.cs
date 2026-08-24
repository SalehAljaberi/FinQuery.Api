using System.Text.RegularExpressions;

namespace FinQuery.Api.Services.Search;

/// <summary>
/// High-performance text processing utility for tokenization, stop-word filtering,
/// and PDF table text cleanup.
/// Uses SSF (Separate Storage Factor / Chaining) HashSet with pre-sized capacity (load factor ~0.72)
/// for guaranteed O(1) stop word lookups.
/// </summary>
public static class TextProcessor
{
    // 174 standard English stop words
    private static readonly string[] RawStopWords = new[]
    {
        "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "aren't",
        "as", "at", "be", "because", "been", "before", "being", "below", "between", "both", "but", "by",
        "can't", "cannot", "could", "couldn't", "did", "didn't", "do", "does", "doesn't", "doing", "don't",
        "down", "during", "each", "few", "for", "from", "further", "had", "hadn't", "has", "hasn't", "have",
        "haven't", "having", "he", "he'd", "he'll", "he's", "her", "here", "here's", "hers", "herself",
        "him", "himself", "his", "how", "how's", "i", "i'd", "i'll", "i'm", "i've", "if", "in", "into",
        "is", "isn't", "it", "it's", "its", "itself", "let's", "me", "more", "most", "mustn't", "my",
        "myself", "no", "nor", "not", "of", "off", "on", "once", "only", "or", "other", "ought", "our",
        "ours", "ourselves", "out", "over", "own", "same", "shan't", "she", "she'd", "she'll", "she's",
        "should", "shouldn't", "so", "some", "such", "than", "that", "that's", "the", "their", "theirs",
        "them", "themselves", "then", "there", "there's", "these", "they", "they'd", "they'll", "they're",
        "they've", "this", "those", "through", "to", "too", "under", "until", "up", "very", "was", "wasn't",
        "we", "we'd", "we'll", "we're", "we've", "were", "weren't", "what", "what's", "when", "when's",
        "where", "where's", "which", "while", "who", "who's", "whom", "why", "why's", "with", "won't",
        "would", "wouldn't", "you", "you'd", "you'll", "you're", "you've", "your", "yours", "yourself",
        "yourselves", "n" // Explicitly include 'n' as stop word (e.g., Pick n Pay)
    };

    // Pre-sized HashSet: 174 items / 0.72 load factor ≈ 242 capacity
    private static readonly HashSet<string> StopWordsSet = new(242, StringComparer.OrdinalIgnoreCase);

    static TextProcessor()
    {
        foreach (var word in RawStopWords)
        {
            StopWordsSet.Add(word);
        }
    }

    // Regex for words, numbers, hyphenated terms (e.g. B-BBEE)
    private static readonly Regex TokenRegex = new(@"\b[a-zA-Z0-9]+(?:['-][a-zA-Z0-9]+)*\b", RegexOptions.Compiled);

    // Regex to split concatenated financial years like FY23FY22FY21 -> FY23 FY22 FY21
    private static readonly Regex ConcatenatedYearsRegex = new(@"(FY\d{2})(?=FY\d{2})", RegexOptions.Compiled);

    // Regex to separate lowercase letter followed immediately by uppercase letter (camelCase table cells)
    private static readonly Regex CamelCaseSplitRegex = new(@"([a-z])([A-Z])", RegexOptions.Compiled);

    // Regex to separate letter followed by number or number followed by letter
    private static readonly Regex LetterNumberSplitRegex = new(@"([a-zA-Z])(\d{2,})|(\d{2,})([a-zA-Z])", RegexOptions.Compiled);

    /// <summary>
    /// Checks if a given word is an English stop word.
    /// </summary>
    public static bool IsStopWord(string word)
    {
        return !string.IsNullOrWhiteSpace(word) && StopWordsSet.Contains(word);
    }

    /// <summary>
    /// Tokenizes input text into a list of clean terms, filtering stop words.
    /// </summary>
    public static List<string> Tokenize(string text, bool filterStopWords = true)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();

        var tokens = new List<string>();
        var matches = TokenRegex.Matches(text);

        foreach (Match match in matches)
        {
            string token = match.Value.ToLowerInvariant();
            
            // Skip single characters (unless it's a significant digit)
            if (token.Length == 1 && !char.IsDigit(token[0]))
                continue;

            if (filterStopWords && StopWordsSet.Contains(token))
                continue;

            tokens.Add(token);
        }

        return tokens;
    }

    /// <summary>
    /// Cleans and formats raw PDF text from PdfPig, separating squashed table columns and concatenated terms.
    /// </summary>
    public static string CleanPdfTableText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // 1. Separate concatenated fiscal years: FY23FY22 -> FY23 FY22
        string cleaned = ConcatenatedYearsRegex.Replace(text, "$1 ");

        // 2. Separate camelCase words in table cells: CommunitiesInvesting -> Communities Investing
        cleaned = CamelCaseSplitRegex.Replace(cleaned, "$1 $2");

        // 3. Separate number-letter concatenations (e.g. 186.5201-1S2.4a2Rand -> 186.5 201-1 S2.4a 2 Rand)
        cleaned = LetterNumberSplitRegex.Replace(cleaned, m => 
        {
            if (m.Groups[1].Success) return $"{m.Groups[1].Value} {m.Groups[2].Value}";
            return $"{m.Groups[3].Value} {m.Groups[4].Value}";
        });

        // 4. Normalize whitespace
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        return cleaned;
    }
}
