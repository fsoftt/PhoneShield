namespace Tranqui.Domain.Users;

/// <summary>Immutable record of a user accepting a specific version of a legal text.</summary>
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
}
