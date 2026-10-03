using Tranqui.Domain.Legal;

namespace Tranqui.Domain.Users;

/// <summary>
/// A registered account. Identity (email, password, Google sign-in) is owned by Firebase; this aggregate only keeps
/// the Firebase uid and the user's consents. It deliberately holds no phone number, name or email.
/// </summary>
public sealed class User
{
    private readonly List<Consent> consents = [];

    private User()
    {
        FirebaseUid = string.Empty;
    }

    private User(string firebaseUid, DateTimeOffset createdAt)
    {
        Id = Guid.CreateVersion7(createdAt);
        FirebaseUid = firebaseUid;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string FirebaseUid { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<Consent> Consents => consents.AsReadOnly();

    public static User Register(string firebaseUid, string acceptedTermsVersion, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firebaseUid);

        if (acceptedTermsVersion != LegalDocuments.CurrentTermsVersion)
        {
            throw new ArgumentException("Only the current terms version can be accepted.", nameof(acceptedTermsVersion));
        }

        var user = new User(firebaseUid, now);
        user.consents.Add(new Consent(ConsentType.TermsAndPrivacyPolicy, acceptedTermsVersion, now));

        return user;
    }

    public void AcceptContactUpload(string acceptedVersion, DateTimeOffset now)
    {
        if (acceptedVersion != LegalDocuments.CurrentContactUploadVersion)
        {
            throw new ArgumentException("Only the current contact upload terms can be accepted.", nameof(acceptedVersion));
        }

        if (AcceptedVersionOf(ConsentType.ContactUpload) == acceptedVersion)
        {
            return;
        }

        RevokeContactUpload(now);
        consents.Add(new Consent(ConsentType.ContactUpload, acceptedVersion, now));
    }

    public void RevokeContactUpload(DateTimeOffset now)
    {
        foreach (var consent in consents.Where(consent => consent.Type == ConsentType.ContactUpload && consent.IsActive))
        {
            consent.Revoke(now);
        }
    }

    public bool HasActiveConsent(ConsentType type, string currentVersion) => AcceptedVersionOf(type) == currentVersion;

    /// <summary>Version of the active (not revoked) consent of a type, or null.</summary>
    public string? AcceptedVersionOf(ConsentType type) =>
        consents.Where(consent => consent.Type == type && consent.IsActive)
            .OrderByDescending(consent => consent.AcceptedAt)
            .Select(consent => consent.Version)
            .FirstOrDefault();
}
