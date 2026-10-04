using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.Reports;

/// <summary>A report the user made, kept on the phone so they can review, change or withdraw it.</summary>
public sealed record MyReport(string E164, ReportVerdictDto Verdict, string? Label, DateTimeOffset ReportedAt);
