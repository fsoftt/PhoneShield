using Tranqui.Domain.Reputation;

namespace Tranqui.Domain.Tests.Reputation;

public sealed class ReporterReputationTests
{
    [Fact]
    public void Multiplier_LittleHistory_IsNeutral()
    {
        ReporterReputation.Multiplier(agreeing: 0, decided: ReporterReputation.MinimumDecidedReports - 1).Should().Be(1);
    }

    [Fact]
    public void Multiplier_AlwaysRight_IsTheMaximum()
    {
        ReporterReputation.Multiplier(10, 10).Should().Be(ReporterReputation.MaximumMultiplier);
    }

    [Fact]
    public void Multiplier_AlwaysWrong_IsTheMinimum()
    {
        ReporterReputation.Multiplier(0, 10).Should().Be(ReporterReputation.MinimumMultiplier);
    }

    [Theory]
    [InlineData(ReportVerdict.Spam, CommunityVerdict.Spam, true)]
    [InlineData(ReportVerdict.NotSpam, CommunityVerdict.Legitimate, true)]
    [InlineData(ReportVerdict.Spam, CommunityVerdict.Legitimate, false)]
    [InlineData(ReportVerdict.NotSpam, CommunityVerdict.Spam, false)]
    public void Agrees_MatchesTheSettledVerdict(ReportVerdict verdict, CommunityVerdict community, bool agrees)
    {
        ReporterReputation.Agrees(verdict, community).Should().Be(agrees);
    }
}
