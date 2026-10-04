namespace Tranqui.Domain.Reputation;

/// <summary>
/// Recognizes insults and slurs in caller names and spam labels, from the versioned list in offensive-words.txt.
/// Matches whole words, plus longer listed words inside a word ("malparidos"), so short words never cause false
/// positives inside unrelated names.
/// </summary>
public static class OffensiveNameFilter
{
    private const int MinimumLengthForPartialMatch = 5;
    private const char CommentMarker = '#';

    private static readonly HashSet<string> words = Load();

    public static bool IsOffensive(CallerName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return name.CanonicalValue
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => new string(word.Where(char.IsLetter).ToArray()))
            .Any(word => words.Contains(word)
                || words.Any(listed => listed.Length >= MinimumLengthForPartialMatch && word.Contains(listed, StringComparison.Ordinal)));
    }

    private static HashSet<string> Load()
    {
        using var stream = typeof(OffensiveNameFilter).Assembly.GetManifestResourceStream("Tranqui.Domain.Reputation.offensive-words.txt")
            ?? throw new InvalidOperationException("The offensive word list is missing from the assembly.");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd()
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && line[0] != CommentMarker)
            .ToHashSet(StringComparer.Ordinal);
    }
}
