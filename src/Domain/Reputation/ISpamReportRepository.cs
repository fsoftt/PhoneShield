using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Reputation;

public interface ISpamReportRepository
{
    Task<SpamReport?> GetAsync(PhoneHash phoneHash, ContributorId contributor, CancellationToken cancellationToken);

    void Add(SpamReport report);

    void Remove(SpamReport report);

    Task<int> CountForContributorAsync(ContributorId contributor, CancellationToken cancellationToken);

    Task<int> RemoveAllForContributorAsync(ContributorId contributor, CancellationToken cancellationToken);
}
