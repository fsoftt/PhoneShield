namespace Tranqui.Application.Features.GetBackOfficeOverview;

public sealed record BackOfficeTotals(
    int Accounts,
    int SpamReports,
    int ContactContributions,
    int HiddenNumbers,
    int ClearedNumbers);
