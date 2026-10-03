using Tranqui.Domain.Reputation;

namespace Tranqui.Domain.Tests.Reputation;

public sealed class ReputationRulesTests
{
    [Fact]
    public void DecayFactor_FreshVote_IsOne()
    {
        ReputationRules.DecayFactor(TimeSpan.Zero).Should().Be(1);
    }

    [Fact]
    public void DecayFactor_AfterOneHalfLife_IsHalf()
    {
        ReputationRules.DecayFactor(ReputationRules.VoteHalfLife).Should().BeApproximately(0.5, 1e-9);
    }

    [Fact]
    public void DecayFactor_WithinTheFirstDay_IsOne()
    {
        ReputationRules.DecayFactor(TimeSpan.FromHours(23)).Should().Be(1);
    }

    [Fact]
    public void DecayFactor_FutureTimestamp_IsCappedAtOne()
    {
        ReputationRules.DecayFactor(TimeSpan.FromDays(-1)).Should().Be(1);
    }

    [Fact]
    public void ReporterWeight_NewAccount_IsReduced()
    {
        ReputationRules.ReporterWeight(TimeSpan.FromDays(1)).Should().Be(ReputationRules.NewAccountVoteWeight);
    }

    [Fact]
    public void ReporterWeight_EstablishedAccount_IsFull()
    {
        ReputationRules.ReporterWeight(ReputationRules.NewAccountPeriod).Should().Be(ReputationRules.EstablishedAccountVoteWeight);
    }
}
