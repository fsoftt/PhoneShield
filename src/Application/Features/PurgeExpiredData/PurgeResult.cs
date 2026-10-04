namespace Tranqui.Application.Features.PurgeExpiredData;

public sealed record PurgeResult(int AppealQuotaUsages, int ResolvedAppeals, int SpamReports, int BlockSignals)
{
    public int Total => AppealQuotaUsages + ResolvedAppeals + SpamReports + BlockSignals;
}
