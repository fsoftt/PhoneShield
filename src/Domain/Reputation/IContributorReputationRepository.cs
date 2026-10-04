namespace Tranqui.Domain.Reputation;

public interface IContributorReputationRepository
{
    /// <summary>Replaces every stored reputation with <paramref name="reputations"/> (contributors left out count at 1).</summary>
    Task ReplaceAllAsync(IReadOnlyCollection<ContributorReputation> reputations, CancellationToken cancellationToken);

    Task RemoveAsync(ContributorId contributor, CancellationToken cancellationToken);
}
