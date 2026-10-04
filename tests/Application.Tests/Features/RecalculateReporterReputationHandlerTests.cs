using System.Security.Cryptography;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Tranqui.Application.Features.RecalculateReporterReputation;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Tests.Features;

public sealed class RecalculateReporterReputationHandlerTests
{
    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

    private readonly IReporterEvidenceReader evidence = Substitute.For<IReporterEvidenceReader>();
    private readonly IContributorReputationRepository reputations = Substitute.For<IContributorReputationRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_ReporterAlwaysAgainstTheCommunity_GetsTheMinimumMultiplier()
    {
        var contrarian = NewContributor();
        // Five spam numbers, each flagged by many people, which the contrarian always calls "not spam".
        var numbers = Enumerable.Range(0, ReporterReputation.MinimumDecidedReports)
            .Select(_ => new ReputationSignals(
                [.. SpamVotesFromOthers(), new SpamVote(ReportVerdict.NotSpam, 1, now, contrarian)], [], 0))
            .ToList();
        evidence.ReadAllAsync(Arg.Any<CancellationToken>()).Returns(numbers.ToAsyncEnumerable());
        IReadOnlyCollection<ContributorReputation>? saved = null;
        await reputations.ReplaceAllAsync(Arg.Do<IReadOnlyCollection<ContributorReputation>>(list => saved = list), Arg.Any<CancellationToken>());

        await new RecalculateReporterReputationHandler(evidence, reputations, unitOfWork, new FakeTimeProvider(now))
            .Handle(new RecalculateReporterReputationCommand(), CancellationToken.None);

        var contrarianId = contrarian.Value.ToArray();
        saved.Should().ContainSingle(reputation => reputation.ContributorId.SequenceEqual(contrarianId))
            .Which.Multiplier.Should().Be(ReporterReputation.MinimumMultiplier);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UndecidedNumbers_RateNobody()
    {
        evidence.ReadAllAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { new ReputationSignals([new SpamVote(ReportVerdict.Spam, 1, now, NewContributor())], [], 0) }.ToAsyncEnumerable());
        IReadOnlyCollection<ContributorReputation>? saved = null;
        await reputations.ReplaceAllAsync(Arg.Do<IReadOnlyCollection<ContributorReputation>>(list => saved = list), Arg.Any<CancellationToken>());

        var rated = await new RecalculateReporterReputationHandler(evidence, reputations, unitOfWork, new FakeTimeProvider(now))
            .Handle(new RecalculateReporterReputationCommand(), CancellationToken.None);

        rated.Should().Be(0);
        saved.Should().BeEmpty();
    }

    private static IEnumerable<SpamVote> SpamVotesFromOthers() =>
        Enumerable.Range(0, 3).SelectMany(day => Enumerable.Range(0, 3)
            .Select(_ => new SpamVote(ReportVerdict.Spam, 1, now.AddDays(-day), NewContributor())));

    private static ContributorId NewContributor() => new(RandomNumberGenerator.GetBytes(ContributorId.SizeInBytes));
}
