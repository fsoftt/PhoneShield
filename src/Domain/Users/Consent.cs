namespace Tranqui.Domain.Users;

/// <summary>Record of a user accepting a specific version of a legal text; it is revoked, never deleted.</summary>
public sealed class Consent
{
    private Consent()
    {
        Version = string.Empty;
    }

    internal Consent(ConsentType type, string version, DateTimeOffset acceptedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        Type = type;
        Version = version;
        AcceptedAt = acceptedAt;
    }

    public ConsentType Type { get; private set; }

    public string Version { get; private set; }

    public DateTimeOffset AcceptedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt is null;

    internal void Revoke(DateTimeOffset revokedAt) => RevokedAt ??= revokedAt;
}
