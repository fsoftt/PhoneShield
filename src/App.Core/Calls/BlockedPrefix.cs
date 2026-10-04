namespace Tranqui.App.Core.Calls;

/// <summary>
/// Turns what the user typed into an E.164 prefix: "+1" stays international, "601" (a Bogotá landline) becomes
/// "+57601" because numbers without a country code are Colombian.
/// </summary>
public static class BlockedPrefix
{
    /// <summary>"+1" (North America) is a valid prefix; without a country code at least two digits are needed.</summary>
    private const int MinimumInternationalDigits = 1;
    private const int MinimumDigits = 2;
    private const int MaximumDigits = 12;

    public static string? TryNormalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var trimmed = raw.Trim();
        var international = trimmed.TrimStart('(', ' ').StartsWith('+');
        var digits = new string(trimmed.Where(char.IsAsciiDigit).ToArray());
        if (trimmed.Any(character => !char.IsAsciiDigit(character) && character is not ('+' or ' ' or '-' or '(' or ')'))
            || digits.Length < (international ? MinimumInternationalDigits : MinimumDigits)
            || digits.Length > MaximumDigits)
        {
            return null;
        }

        return international ? "+" + digits : $"+{CallScreener.HomeCountryCode}{digits.TrimStart('0')}";
    }
}
