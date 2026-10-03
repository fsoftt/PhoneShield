using NumberParseException = global::PhoneNumbers.NumberParseException;
using PhoneNumberFormat = global::PhoneNumbers.PhoneNumberFormat;
using PhoneNumberUtil = global::PhoneNumbers.PhoneNumberUtil;

namespace Tranqui.Domain.PhoneNumbers;

/// <summary>
/// A valid phone number normalized to E.164. This is the only place in the system where raw numbers are normalized.
/// <see cref="ToString"/> is masked so the full number never leaks into logs, exceptions or interpolated strings;
/// read <see cref="E164"/> explicitly where the full value is genuinely needed (hashing, dialing).
/// </summary>
public sealed record PhoneNumber
{
    public const string DefaultRegion = "CO";

    private const int VisibleTrailingDigits = 3;
    private const char MaskCharacter = '•';

    private static readonly PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();

    private PhoneNumber(string e164, int countryCode)
    {
        E164 = e164;
        CountryCode = countryCode;
    }

    public string E164 { get; }

    public int CountryCode { get; }

    /// <summary>Parses and normalizes a raw number as typed or stored on a device. Returns null when it is not a valid number.</summary>
    public static PhoneNumber? TryParse(string? raw, string defaultRegion = DefaultRegion)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        try
        {
            var parsed = phoneNumberUtil.Parse(raw, defaultRegion);
            if (!phoneNumberUtil.IsValidNumber(parsed))
            {
                return null;
            }

            return new PhoneNumber(phoneNumberUtil.Format(parsed, PhoneNumberFormat.E164), parsed.CountryCode);
        }
        catch (NumberParseException)
        {
            return null;
        }
    }

    /// <summary>Country code plus the last digits only, e.g. "+57 •••••••567".</summary>
    public string Masked
    {
        get
        {
            var prefix = $"+{CountryCode}";
            var nationalLength = E164.Length - prefix.Length;
            var hiddenLength = Math.Max(0, nationalLength - VisibleTrailingDigits);

            return $"{prefix} {new string(MaskCharacter, hiddenLength)}{E164[^Math.Min(VisibleTrailingDigits, nationalLength)..]}";
        }
    }

    public override string ToString() => Masked;
}
