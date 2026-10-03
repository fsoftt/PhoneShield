using System.Globalization;
using System.Text;

namespace Tranqui.Domain.Reputation;

/// <summary>A caller name or spam label as a person typed it, plus the canonical form used to group equal names.</summary>
public sealed record CallerName
{
    public const int MaxLength = 60;

    private CallerName(string displayValue, string canonicalValue)
    {
        DisplayValue = displayValue;
        CanonicalValue = canonicalValue;
    }

    /// <summary>Trimmed text with inner whitespace collapsed, as shown to users.</summary>
    public string DisplayValue { get; }

    /// <summary>Lowercase, accent-free form: "Spam  Claro" and "spam claro" are the same name.</summary>
    public string CanonicalValue { get; }

    public static CallerName? TryCreate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var display = string.Join(' ', raw.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (display.Length > MaxLength)
        {
            return null;
        }

        return new CallerName(display, Canonicalize(display));
    }

    private static string Canonicalize(string value)
    {
        var decomposed = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
        {
            builder.Append(character);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
