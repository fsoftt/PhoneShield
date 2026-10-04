namespace Tranqui.BackOffice;

public sealed class BackOfficeOptions
{
    public const string SectionName = "BackOffice";

    /// <summary>The Tranqui API. In production the back office reaches it over the internal Docker network.</summary>
    public Uri? ApiBaseUrl { get; set; }

    /// <summary>Firebase Web API key used to sign in with email and password. Public, not a secret.</summary>
    public string FirebaseApiKey { get; set; } = string.Empty;

    /// <summary>Where the cookie encryption keys are kept, so sessions survive restarts. Empty: in memory.</summary>
    public string? DataProtectionKeysPath { get; set; }
}
