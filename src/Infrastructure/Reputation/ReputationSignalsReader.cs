using Microsoft.EntityFrameworkCore;
using Tranqui.Application.Features.LookupNumber;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Infrastructure.Reputation;

internal sealed class ReputationSignalsReader(TranquiDbContext dbContext) : IReputationSignalsReader
{
    public async Task<ReputationSignals> ReadAsync(PhoneHash phoneHash, CancellationToken cancellationToken)
    {
        var hash = phoneHash.Value.ToArray();

        var reports = await dbContext.SpamReports.AsNoTracking()
            .Where(report => report.PhoneHash == hash)
            .ToListAsync(cancellationToken);
        var contributions = await dbContext.ContactContributions.AsNoTracking()
            .Where(contribution => contribution.PhoneHash == hash)
            .ToListAsync(cancellationToken);

        var spamVotes = reports.Select(report => new SpamVote(report.Verdict, report.Weight, report.ReportedAt)).ToList();
        var nameVotes = contributions
            .Where(contribution => contribution.Name is not null)
            .Select(contribution => new NameVote(
                new ContributorId(contribution.ContributorId), contribution.Name!, contribution.ContributedAt))
            .Concat(reports
                .Where(report => report.Label is not null)
                .Select(report => new NameVote(new ContributorId(report.ContributorId), report.Label!, report.ReportedAt)))
            .ToList();

        return new ReputationSignals(spamVotes, nameVotes, contributions.Count);
    }
}
