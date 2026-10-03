namespace Tranqui.Infrastructure.Integrity;

public sealed class PlayIntegrityOptions
{
    public const string SectionName = "PlayIntegrity";

    /// <summary>The app's package name; verdicts for any other package are rejected.</summary>
    public string PackageName { get; set; } = "com.fsoftt.tranqui";

    /// <summary>
    /// Base64 of the JSON key of a Google Cloud service account allowed to decode integrity tokens. Secret: never commit
    /// it. Without it every integrity check fails, so appeals stay closed.
    /// </summary>
    public string ServiceAccountKey { get; set; } = string.Empty;
}
