using MediatR;
using Tranqui.Domain.Appeals;

namespace Tranqui.Application.Features.GetBackOfficeOverview;

internal sealed class GetBackOfficeOverviewHandler(
    IBackOfficeStatistics statistics,
    IAppealRepository appeals,
    TimeProvider timeProvider) : IRequestHandler<GetBackOfficeOverviewQuery, BackOfficeOverview>
{
    /// <summary>Pending reviews are few (a handful per day at most); this bounds the scan anyway.</summary>
    private const int MaxPendingScanned = 1_000;

    public async Task<BackOfficeOverview> Handle(GetBackOfficeOverviewQuery request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var totals = await statistics.ReadAsync(cancellationToken);
        var pending = await appeals.ListAppealsAsync(AppealStatus.Pending, MaxPendingScanned, cancellationToken);

        return new BackOfficeOverview(
            totals.Accounts,
            totals.SpamReports,
            totals.ContactContributions,
            pending.Count,
            pending.Count(appeal => now > appeal.DueAt),
            totals.HiddenNumbers,
            totals.ClearedNumbers);
    }
}
