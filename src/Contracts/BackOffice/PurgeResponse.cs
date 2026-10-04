namespace Tranqui.Contracts.BackOffice;

public sealed record PurgeResponse(int AppealQuotaUsages, int ResolvedAppeals, int SpamReports, int BlockSignals);
