namespace Tranqui.Domain.Reputation;

/// <summary>
/// Reporter reliability (spec §4.3): reports that match the community's settled verdict raise a reporter's weight,
/// reports against it lower it. Too little history leaves the weight at 1.
/// </summary>
public static class ReporterReputation
{
    public const int MinimumDecidedReports = 5;
    public const double MinimumMultiplier = 0.25;
    public const double MaximumMultiplier = 1.5;

    /// <param name="agreeing">Reports matching the verdict of numbers the community has decided.</param>
    /// <param name="decided">Reports on numbers the community has decided.</param>
    public static double Multiplier(int agreeing, int decided)
    {
        if (decided < MinimumDecidedReports)
        {
            return 1;
        }

        var agreement = (double)agreeing / decided;

        return Math.Clamp(MinimumMultiplier + ((MaximumMultiplier - MinimumMultiplier) * agreement), MinimumMultiplier, MaximumMultiplier);
    }

    public static bool Agrees(ReportVerdict verdict, CommunityVerdict community) => (verdict, community) switch
    {
        (ReportVerdict.Spam, CommunityVerdict.Spam) => true,
        (ReportVerdict.NotSpam, CommunityVerdict.Legitimate) => true,
        _ => false,
    };
}
