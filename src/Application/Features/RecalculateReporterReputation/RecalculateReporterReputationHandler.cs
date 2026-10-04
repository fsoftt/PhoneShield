using MediatR;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.RecalculateReporterReputation;

internal sealed class RecalculateReporterReputationHandler(
    IReporterEvidenceReader evidence,
    IContributorReputationRepository reputations,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<RecalculateReporterReputationCommand, int>
{
    public async Task<int> Handle(RecalculateReporterReputationCommand request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var tallies = new Dictionary<string, Tally>(StringComparer.Ordinal);

        await foreach (var signals in evidence.ReadAllAsync(cancellationToken))
        {
            var verdict = CallerIdentification.VerdictOf(signals, now);
            if (verdict == CommunityVerdict.Undecided)
            {
                continue;
            }

            foreach (var vote in signals.SpamVotes.Where(vote => vote.Contributor is not null))
            {
                var key = Convert.ToHexString(vote.Contributor!.Value);
                var tally = tallies.GetValueOrDefault(key) ?? new Tally(vote.Contributor);
                tally.Decided++;
                if (ReporterReputation.Agrees(vote.Verdict, verdict))
                {
                    tally.Agreeing++;
                }

                tallies[key] = tally;
            }
        }

        var rated = tallies.Values
            .Select(tally => ContributorReputation.Create(
                tally.Contributor, ReporterReputation.Multiplier(tally.Agreeing, tally.Decided), now))
            .Where(reputation => Math.Abs(reputation.Multiplier - 1) > double.Epsilon)
            .ToList();
        await reputations.ReplaceAllAsync(rated, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return rated.Count;
    }

    private sealed class Tally(ContributorId contributor)
    {
        public ContributorId Contributor { get; } = contributor;

        public int Agreeing { get; set; }

        public int Decided { get; set; }
    }
}
