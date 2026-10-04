namespace Tranqui.Application.Features.PurgeExpiredData;

public sealed record PurgeResult(int AppealQuotaUsages, int ResolvedAppeals, int SpamReports)
{
    public int Total => AppealQuotaUsages + ResolvedAppeals + SpamReports;
}
