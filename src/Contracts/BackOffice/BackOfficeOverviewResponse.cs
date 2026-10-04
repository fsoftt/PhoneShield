namespace Tranqui.Contracts.BackOffice;

public sealed record BackOfficeOverviewResponse(
    int Accounts,
    int SpamReports,
    int ContactContributions,
    int PendingAppeals,
    int OverdueAppeals,
    int HiddenNumbers,
    int ClearedNumbers);
