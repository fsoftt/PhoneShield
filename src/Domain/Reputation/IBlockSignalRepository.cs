using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Reputation;

public interface IBlockSignalRepository
{
    Task<BlockSignal?> GetAsync(PhoneHash phoneHash, ContributorId contributor, CancellationToken cancellationToken);

    void Add(BlockSignal signal);

    void Remove(BlockSignal signal);

    Task<int> CountForContributorAsync(ContributorId contributor, CancellationToken cancellationToken);

    Task<int> RemoveAllForContributorAsync(ContributorId contributor, CancellationToken cancellationToken);
}
