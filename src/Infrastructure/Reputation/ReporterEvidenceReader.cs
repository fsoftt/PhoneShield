using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Tranqui.Application.Features.RecalculateReporterReputation;
using Tranqui.Domain.Reputation;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Infrastructure.Reputation;

/// <summary>
/// Loads every report, block and saved-by count, grouped per number. Fine at v1 scale (one daily pass); move to SQL
/// aggregation if the report table grows into the millions.
/// </summary>
internal sealed class ReporterEvidenceReader(TranquiDbContext dbContext) : IReporterEvidenceReader
{
    public async IAsyncEnumerable<ReputationSignals> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var reports = await dbContext.SpamReports.AsNoTracking()
            .Select(report => new { report.PhoneHash, report.ContributorId, report.Verdict, report.Weight, report.ReportedAt })
            .ToListAsync(cancellationToken);
        var blocks = (await dbContext.BlockSignals.AsNoTracking()
                .Select(block => new { block.PhoneHash, block.Weight, block.BlockedAt })
                .ToListAsync(cancellationToken))
            .ToLookup(block => Convert.ToHexString(block.PhoneHash));
        var savedBy = (await dbContext.ContactContributions.AsNoTracking()
                .GroupBy(contribution => contribution.PhoneHash)
                .Select(group => new { PhoneHash = group.Key, Count = group.Count() })
                .ToListAsync(cancellationToken))
            .ToDictionary(row => Convert.ToHexString(row.PhoneHash), row => row.Count);
        var cleared = (await dbContext.ClearedNumbers.AsNoTracking().ToListAsync(cancellationToken))
            .ToDictionary(row => Convert.ToHexString(row.PhoneHash), row => row.ClearedAt);

        foreach (var number in reports.GroupBy(report => Convert.ToHexString(report.PhoneHash)))
        {
            var signals = new ReputationSignals(
                number.Select(report => new SpamVote(report.Verdict, report.Weight, report.ReportedAt, new ContributorId(report.ContributorId)))
                    .ToList(),
                [],
                savedBy.GetValueOrDefault(number.Key))
            {
                BlockVotes = blocks[number.Key].Select(block => new BlockVote(block.Weight, block.BlockedAt)).ToList(),
            };

            yield return cleared.TryGetValue(number.Key, out var clearedAt) ? signals.WithSpamClearedBefore(clearedAt) : signals;
        }
    }
}
