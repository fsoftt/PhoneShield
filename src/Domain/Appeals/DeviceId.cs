namespace Tranqui.Domain.Appeals;

/// <summary>
/// The app's per-device identifier (Android's ANDROID_ID: per device, user and signing key; reset only by a factory
/// reset). Never stored: the server keeps only a keyed hash of it to count appeals per device.
/// </summary>
public sealed record DeviceId
{
    public const int MaxLength = 64;

    private DeviceId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DeviceId? TryParse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || raw.Length > MaxLength || !raw.All(char.IsAsciiLetterOrDigit))
        {
            return null;
        }

        return new DeviceId(raw.ToLowerInvariant());
    }

    public override string ToString() => "DeviceId(***)";
}
