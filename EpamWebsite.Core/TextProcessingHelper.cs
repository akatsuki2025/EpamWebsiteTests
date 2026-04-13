namespace EpamWebsite.Core;

public static class TextProcessingHelper
{
    public static IEnumerable<string> ExtractNormalizedWords(string text)
    {
        return System.Text.RegularExpressions.Regex.Split(text ?? string.Empty, @"\W+")
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select(w => w.ToLowerInvariant());
    }
}
