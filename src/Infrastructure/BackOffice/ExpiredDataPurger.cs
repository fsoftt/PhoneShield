using Microsoft.EntityFrameworkCore;
using Tranqui.Application.Features.PurgeExpiredData;
using Tranqui.Domain.Appeals;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Infrastructure.BackOffice;

/// <summary>Set-based deletes: nothing is loaded into memory.</summary>
internal sealed class ExpiredDataPurger(TranquiDbContext dbContext) : IExpiredDataPurger
{
    public async Task<PurgeResult> PurgeAsync(PurgeCutoffs cutoffs, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cutoffs);

        var quotaUsages = await dbContext.AppealQuotaUsages
            .Where(usage => usage.UsedAt < cutoffs.AppealQuotaUsages)
            .ExecuteDeleteAsync(cancellationToken);

        // Pending reviews are never purged: they still need an answer.
        var resolvedAppeals = await dbContext.Appeals
            .Where(appeal => appeal.Status != AppealStatus.Pending
                && (appeal.ResolvedAt ?? appeal.CreatedAt) < cutoffs.ResolvedAppeals)
            .ExecuteDeleteAsync(cancellationToken);

        var spamReports = await dbContext.SpamReports
            .Where(report => report.ReportedAt < cutoffs.Votes)
            .ExecuteDeleteAsync(cancellationToken);

        return new PurgeResult(quotaUsages, resolvedAppeals, spamReports);
    }
}
