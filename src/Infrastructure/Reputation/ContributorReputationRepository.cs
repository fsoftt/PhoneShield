using Microsoft.EntityFrameworkCore;
using Tranqui.Domain.Reputation;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Infrastructure.Reputation;

internal sealed class ContributorReputationRepository(TranquiDbContext dbContext) : IContributorReputationRepository
{
    public async Task ReplaceAllAsync(IReadOnlyCollection<ContributorReputation> reputations, CancellationToken cancellationToken)
    {
        await dbContext.ContributorReputations.ExecuteDeleteAsync(cancellationToken);
        dbContext.ContributorReputations.AddRange(reputations);
    }

    public Task RemoveAsync(ContributorId contributor, CancellationToken cancellationToken)
    {
        var contributorId = contributor.Value.ToArray();

        return dbContext.ContributorReputations
            .Where(reputation => reputation.ContributorId == contributorId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
