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
        var namesHidden = await dbContext.HiddenNumbers.AnyAsync(hidden => hidden.PhoneHash == hash, cancellationToken);
        var clearedAt = await dbContext.ClearedNumbers.AsNoTracking()
            .Where(cleared => cleared.PhoneHash == hash)
            .Select(cleared => (DateTimeOffset?)cleared.ClearedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var reporters = reports.Select(report => report.ContributorId).ToList();
        var multipliers = (await dbContext.ContributorReputations.AsNoTracking()
                .Where(reputation => reporters.Contains(reputation.ContributorId))
                .ToListAsync(cancellationToken))
            .ToDictionary(reputation => Convert.ToHexString(reputation.ContributorId), reputation => reputation.Multiplier);
        var blocks = await dbContext.BlockSignals.AsNoTracking()
            .Where(block => block.PhoneHash == hash)
            .Select(block => new BlockVote(block.Weight, block.BlockedAt))
            .ToListAsync(cancellationToken);

        // Each vote counts with its reporter's reliability (1 for reporters without enough history).
        var spamVotes = reports
            .Select(report => new SpamVote(
                report.Verdict,
                report.Weight * multipliers.GetValueOrDefault(Convert.ToHexString(report.ContributorId), 1),
                report.ReportedAt))
            .ToList();
        // The owner asked to hide names: they are never shown, but spam votes and the saved-by count still apply.
        var nameVotes = namesHidden ? [] : contributions
            .Where(contribution => contribution.Name is not null)
            .Select(contribution => new NameVote(
                new ContributorId(contribution.ContributorId), contribution.Name!, contribution.ContributedAt))
            .Concat(reports
                .Where(report => report.Label is not null)
                .Select(report => new NameVote(new ContributorId(report.ContributorId), report.Label!, report.ReportedAt)))
            .ToList();

        var signals = new ReputationSignals(spamVotes, nameVotes, contributions.Count) { BlockVotes = blocks };

        return clearedAt is { } cleared ? signals.WithSpamClearedBefore(cleared) : signals;
    }
}
