namespace Tranqui.Application.Features.GetBackOfficeOverview;

/// <summary>Totals only: the back office never lists users, numbers or reports one by one.</summary>
public sealed record BackOfficeOverview(
    int Accounts,
    int SpamReports,
    int ContactContributions,
    int PendingAppeals,
    int OverdueAppeals,
    int HiddenNumbers,
    int ClearedNumbers);
