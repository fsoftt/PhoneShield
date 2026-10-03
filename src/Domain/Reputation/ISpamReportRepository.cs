using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Reputation;

public interface ISpamReportRepository
{
    Task<SpamReport?> GetAsync(PhoneHash phoneHash, ContributorId contributor, CancellationToken cancellationToken);

    void Add(SpamReport report);
}
