using Microsoft.EntityFrameworkCore;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Infrastructure.Reputation;

internal sealed class BlockSignalRepository(TranquiDbContext dbContext) : IBlockSignalRepository
{
    public Task<BlockSignal?> GetAsync(PhoneHash phoneHash, ContributorId contributor, CancellationToken cancellationToken)
    {
        var hash = phoneHash.Value.ToArray();
        var contributorId = contributor.Value.ToArray();

        return dbContext.BlockSignals.FirstOrDefaultAsync(
            block => block.PhoneHash == hash && block.ContributorId == contributorId, cancellationToken);
    }

    public void Add(BlockSignal signal) => dbContext.BlockSignals.Add(signal);

    public void Remove(BlockSignal signal) => dbContext.BlockSignals.Remove(signal);

    public Task<int> CountForContributorAsync(ContributorId contributor, CancellationToken cancellationToken)
    {
        var contributorId = contributor.Value.ToArray();

        return dbContext.BlockSignals.CountAsync(block => block.ContributorId == contributorId, cancellationToken);
    }

    public Task<int> RemoveAllForContributorAsync(ContributorId contributor, CancellationToken cancellationToken)
    {
        var contributorId = contributor.Value.ToArray();

        return dbContext.BlockSignals.Where(block => block.ContributorId == contributorId).ExecuteDeleteAsync(cancellationToken);
    }
}
