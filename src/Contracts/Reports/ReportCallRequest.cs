namespace Tranqui.Contracts.Reports;

public sealed record ReportCallRequest(string PhoneNumber, ReportVerdictDto Verdict, string? Label);
