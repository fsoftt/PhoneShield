namespace Tranqui.Infrastructure.PhoneNumbers;

/// <summary>
/// Secret keys for phone number hashing, indexed by version (1, 2, 3...). Every version from 1 up to
/// <see cref="CurrentKeyVersion"/> must stay configured: rotating a key chains a new HMAC on top of the old hash,
/// because the original numbers are never stored and cannot be re-hashed.
/// </summary>
public sealed class PhoneHashingOptions
{
    public const string SectionName = "PhoneHashing";

    public int CurrentKeyVersion { get; set; }

    /// <summary>Base64-encoded keys by version. Secret: never commit them; use user-secrets or environment variables.</summary>
    public Dictionary<int, string> Keys { get; set; } = [];
}
