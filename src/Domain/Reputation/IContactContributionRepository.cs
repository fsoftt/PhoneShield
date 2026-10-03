namespace Tranqui.Domain.Reputation;

public interface IContactContributionRepository
{
    /// <summary>Existing contributions of a contributor for the given phone hashes, keyed by hex-encoded hash.</summary>
    Task<IReadOnlyDictionary<string, ContactContribution>> GetForContributorAsync(
        ContributorId contributor,
        IReadOnlyCollection<byte[]> phoneHashes,
        CancellationToken cancellationToken);

    Task<int> CountForContributorAsync(ContributorId contributor, CancellationToken cancellationToken);

    void Add(ContactContribution contribution);

    Task<int> RemoveAllForContributorAsync(ContributorId contributor, CancellationToken cancellationToken);
}
