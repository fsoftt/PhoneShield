using MediatR;
using Tranqui.Application.Features.LookupNumber;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.ListAppeals;

internal sealed class ListAppealsHandler(
    IAppealRepository appeals,
    IReputationSignalsReader signalsReader,
    TimeProvider timeProvider) : IRequestHandler<ListAppealsQuery, IReadOnlyList<AppealReview>>
{
    private const int PageSize = 100;

    public async Task<IReadOnlyList<AppealReview>> Handle(ListAppealsQuery request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var page = await appeals.ListAppealsAsync(request.Status, PageSize, cancellationToken);
        var reviews = new List<AppealReview>(page.Count);

        foreach (var appeal in page)
        {
            var signals = await signalsReader.ReadAsync(appeal.Hash, cancellationToken);
            var identification = CallerIdentification.Evaluate(signals, now);

            reviews.Add(new AppealReview(
                appeal.Id,
                appeal.Kind,
                appeal.Status,
                appeal.Reason,
                appeal.ContactEmail,
                appeal.CreatedAt,
                appeal.DueAt,
                appeal.ResolvedAt,
                appeal.Status == AppealStatus.Pending && now > appeal.DueAt,
                identification.Status,
                identification.SpamReportCount,
                signals.SpamVotes.Count(vote => vote.Verdict == ReportVerdict.NotSpam),
                identification.SavedByCount));
        }

        return reviews;
    }
}
