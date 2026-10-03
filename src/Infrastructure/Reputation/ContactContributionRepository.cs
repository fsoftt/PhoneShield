using Microsoft.EntityFrameworkCore;
using Tranqui.Domain.Reputation;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Infrastructure.Reputation;

internal sealed class ContactContributionRepository(TranquiDbContext dbContext) : IContactContributionRepository
{
    public async Task<IReadOnlyDictionary<string, ContactContribution>> GetForContributorAsync(
        ContributorId contributor,
        IReadOnlyCollection<byte[]> phoneHashes,
        CancellationToken cancellationToken)
    {
        var contributorId = contributor.Value.ToArray();

        var existing = await dbContext.ContactContributions
            .Where(contribution => contribution.ContributorId == contributorId && phoneHashes.Contains(contribution.PhoneHash))
            .ToListAsync(cancellationToken);

        return existing.ToDictionary(contribution => Convert.ToHexStringLower(contribution.PhoneHash));
    }

    public Task<int> CountForContributorAsync(ContributorId contributor, CancellationToken cancellationToken)
    {
        var contributorId = contributor.Value.ToArray();

        return dbContext.ContactContributions.CountAsync(
            contribution => contribution.ContributorId == contributorId, cancellationToken);
    }

    public void Add(ContactContribution contribution) => dbContext.ContactContributions.Add(contribution);

    public Task<int> RemoveAllForContributorAsync(ContributorId contributor, CancellationToken cancellationToken)
    {
        var contributorId = contributor.Value.ToArray();

        return dbContext.ContactContributions
            .Where(contribution => contribution.ContributorId == contributorId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
