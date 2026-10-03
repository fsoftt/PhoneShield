using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Reputation;

/// <summary>
/// A number found in a contributor's address book. The name is absent when it was filtered as personal
/// ("Mamá", "Amor"...): the number still counts as "saved by someone", a trust signal against spam.
/// </summary>
public sealed class ContactContribution
{
    private ContactContribution()
    {
        PhoneHash = [];
        ContributorId = [];
    }

    private ContactContribution(PhoneHash phoneHash, ContributorId contributor, ProtectedName? name, DateTimeOffset contributedAt)
    {
        Id = Guid.CreateVersion7(contributedAt);
        PhoneHash = phoneHash.Value.ToArray();
        HashKeyVersion = phoneHash.KeyVersion;
        ContributorId = contributor.Value.ToArray();
        NameCiphertext = name?.Ciphertext;
        NameGroupingKey = name?.GroupingKey;
        ContributedAt = contributedAt;
    }

    public Guid Id { get; private set; }

    public byte[] PhoneHash { get; private set; }

    public int HashKeyVersion { get; private set; }

    public byte[] ContributorId { get; private set; }

    public byte[]? NameCiphertext { get; private set; }

    public byte[]? NameGroupingKey { get; private set; }

    public DateTimeOffset ContributedAt { get; private set; }

    public ProtectedName? Name =>
        NameCiphertext is null || NameGroupingKey is null ? null : new ProtectedName(NameCiphertext, NameGroupingKey);

    /// <summary>The contributor saved the number under another name (or a personal one) since the last upload.</summary>
    public void Rename(ProtectedName? name, DateTimeOffset contributedAt)
    {
        NameCiphertext = name?.Ciphertext;
        NameGroupingKey = name?.GroupingKey;
        ContributedAt = contributedAt;
    }

    public static ContactContribution Create(
        PhoneHash phoneHash,
        ContributorId contributor,
        ProtectedName? name,
        DateTimeOffset contributedAt)
    {
        ArgumentNullException.ThrowIfNull(phoneHash);
        ArgumentNullException.ThrowIfNull(contributor);

        return new ContactContribution(phoneHash, contributor, name, contributedAt);
    }
}
