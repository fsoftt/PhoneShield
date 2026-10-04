using System.Security.Cryptography;
using Tranqui.Domain.Reputation;

namespace Tranqui.Domain.Tests.Reputation;

public sealed class CallerIdentificationTests
{
    private const double FreshWeight = 1;

    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Evaluate_NoSignals_IsUnknown()
    {
        var result = CallerIdentification.Evaluate(ReputationSignals.None, now);

        result.Status.Should().Be(CallerStatus.Unknown);
        result.RankedNames.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_FewerSpamReportsThanThreshold_IsNotSpam()
    {
        var signals = new ReputationSignals(SpamVotes(4), [], 0);

        CallerIdentification.Evaluate(signals, now).Status.Should().Be(CallerStatus.Unknown);
    }

    [Fact]
    public void Evaluate_EnoughSpamReports_IsSpam()
    {
        var result = CallerIdentification.Evaluate(new ReputationSignals(SpamVotes(sameDayVotesNeeded), [], 0), now);

        result.Status.Should().Be(CallerStatus.Spam);
        result.SpamReportCount.Should().Be(sameDayVotesNeeded);
    }

    [Fact]
    public void Evaluate_EnoughSpamReportsFromMinutesAgo_IsSpam()
    {
        var signals = new ReputationSignals(SpamVotes(sameDayVotesNeeded, castAt: now.AddMinutes(-5)), [], 0);

        CallerIdentification.Evaluate(signals, now).Status.Should().Be(CallerStatus.Spam);
    }

    [Fact]
    public void Evaluate_NumberSavedByManyPeople_ResistsAFewSpamReports()
    {
        var signals = new ReputationSignals(SpamVotes(5), [], SavedByCount: 15);

        CallerIdentification.Evaluate(signals, now).Status.Should().NotBe(CallerStatus.Spam);
    }

    [Fact]
    public void Evaluate_NotSpamVotes_OffsetSpamReports()
    {
        var votes = SpamVotes(6).Concat(Enumerable.Repeat(new SpamVote(ReportVerdict.NotSpam, FreshWeight, now), 4)).ToList();

        CallerIdentification.Evaluate(new ReputationSignals(votes, [], 0), now).Status.Should().NotBe(CallerStatus.Spam);
    }

    [Fact]
    public void Evaluate_OldSpamReports_DecayBelowThreshold()
    {
        var oldVotes = SpamVotes(6, castAt: now - ReputationRules.VoteHalfLife);

        CallerIdentification.Evaluate(new ReputationSignals(oldVotes, [], 0), now).Status.Should().NotBe(CallerStatus.Spam);
    }

    [Fact]
    public void Evaluate_NameUsedByFewerPeopleThanThreshold_IsNotShown()
    {
        var name = Name(1);
        var votes = Enumerable.Range(0, ReputationRules.MinimumDistinctContributorsPerName - 1)
            .Select(_ => new NameVote(NewContributor(), name, now))
            .ToList();

        var result = CallerIdentification.Evaluate(new ReputationSignals([], votes, votes.Count), now);

        result.Status.Should().Be(CallerStatus.Unknown);
        result.RankedNames.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_SameContributorRepeatingAName_CountsOnce()
    {
        var contributor = NewContributor();
        var votes = Enumerable.Repeat(new NameVote(contributor, Name(1), now), 5).ToList();

        CallerIdentification.Evaluate(new ReputationSignals([], votes, 1), now).RankedNames.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_NamesAboveThreshold_AreRankedByUsage()
    {
        var common = Name(1);
        var lessCommon = Name(2);
        var votes = Votes(common, 5).Concat(Votes(lessCommon, 3)).ToList();

        var result = CallerIdentification.Evaluate(new ReputationSignals([], votes, 8), now);

        result.Status.Should().Be(CallerStatus.Identified);
        result.RankedNames.Should().Equal(common, lessCommon);
    }

    [Fact]
    public void Evaluate_SpamWithLabels_IsSpamAndKeepsTheLabels()
    {
        var label = Name(1);
        var signals = new ReputationSignals(SpamVotes(sameDayVotesNeeded), Votes(label, 3), 0);

        var result = CallerIdentification.Evaluate(signals, now);

        result.Status.Should().Be(CallerStatus.Spam);
        result.RankedNames.Should().ContainSingle().Which.Should().Be(label);
    }

    [Fact]
    public void Evaluate_BurstOfReportsInOneDay_IsDampened()
    {
        var signals = new ReputationSignals(SpamVotes(sameDayVotesNeeded - 1), [], 0);

        CallerIdentification.Evaluate(signals, now).Status.Should().NotBe(CallerStatus.Spam);
    }

    [Fact]
    public void Evaluate_ReportsSpreadOverDays_ReachSpamWithFewerVotes()
    {
        var votes = SpamVotes(3).Concat(SpamVotes(3, castAt: now.AddDays(-1))).ToList();

        CallerIdentification.Evaluate(new ReputationSignals(votes, [], 0), now).Status.Should().Be(CallerStatus.Spam);
    }

    [Fact]
    public void Evaluate_BlocksAddALittleSpamWeight()
    {
        var votes = SpamVotes(3).Concat(SpamVotes(1, castAt: now.AddDays(-1))).ToList();
        var withoutBlocks = new ReputationSignals(votes, [], 0);
        var withBlocks = withoutBlocks with
        {
            BlockVotes = Enumerable.Repeat(new BlockVote(FreshWeight, now.AddDays(-2)), 5).ToList(),
        };

        CallerIdentification.Evaluate(withoutBlocks, now).Status.Should().NotBe(CallerStatus.Spam);
        CallerIdentification.Evaluate(withBlocks, now).Status.Should().Be(CallerStatus.Spam);
    }

    [Fact]
    public void VerdictOf_SavedAndVouchedNumber_IsLegitimate()
    {
        var votes = Enumerable.Repeat(new SpamVote(ReportVerdict.NotSpam, FreshWeight, now), 2).ToList();

        CallerIdentification.VerdictOf(new ReputationSignals(votes, [], SavedByCount: 3), now).Should().Be(CommunityVerdict.Legitimate);
    }

    [Fact]
    public void VerdictOf_LittleEvidence_IsUndecided()
    {
        CallerIdentification.VerdictOf(new ReputationSignals(SpamVotes(2), [], 0), now).Should().Be(CommunityVerdict.Undecided);
    }

    /// <summary>Fresh reports needed in a single day to flag a number once bursts are dampened.</summary>
    private static int sameDayVotesNeeded =>
        Enumerable.Range(1, 100).First(count => ReputationRules.DampenBurst(count * FreshWeight) >= ReputationRules.MinimumSpamWeight);

    private static List<SpamVote> SpamVotes(int count, DateTimeOffset? castAt = null) =>
        Enumerable.Repeat(new SpamVote(ReportVerdict.Spam, FreshWeight, castAt ?? now), count).ToList();

    private static List<NameVote> Votes(ProtectedName name, int contributors) =>
        Enumerable.Range(0, contributors).Select(_ => new NameVote(NewContributor(), name, now)).ToList();

    private static ProtectedName Name(byte seed) => new([seed], [seed]);

    private static ContributorId NewContributor() => new(RandomNumberGenerator.GetBytes(ContributorId.SizeInBytes));
}
