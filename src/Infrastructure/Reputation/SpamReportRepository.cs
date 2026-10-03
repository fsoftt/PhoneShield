using Microsoft.EntityFrameworkCore;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Infrastructure.Reputation;

internal sealed class SpamReportRepository(TranquiDbContext dbContext) : ISpamReportRepository
{
    public Task<SpamReport?> GetAsync(PhoneHash phoneHash, ContributorId contributor, CancellationToken cancellationToken)
    {
        var hash = phoneHash.Value.ToArray();
        var contributorId = contributor.Value.ToArray();

        return dbContext.SpamReports.FirstOrDefaultAsync(
            report => report.PhoneHash == hash && report.ContributorId == contributorId,
            cancellationToken);
    }

    public void Add(SpamReport report) => dbContext.SpamReports.Add(report);

    public Task<int> CountForContributorAsync(ContributorId contributor, CancellationToken cancellationToken)
    {
        var contributorId = contributor.Value.ToArray();

        return dbContext.SpamReports.CountAsync(report => report.ContributorId == contributorId, cancellationToken);
    }

    public Task<int> RemoveAllForContributorAsync(ContributorId contributor, CancellationToken cancellationToken)
    {
        var contributorId = contributor.Value.ToArray();

        return dbContext.SpamReports.Where(report => report.ContributorId == contributorId).ExecuteDeleteAsync(cancellationToken);
    }
}
