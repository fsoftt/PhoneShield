using Tranqui.App.Core.Sync;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.Reports;

/// <summary>Every way of reporting (history, notification, "My reports") goes through here: saved locally, then sent.</summary>
public sealed class ReportService(IMyReports myReports, Outbox outbox, TimeProvider timeProvider)
{
    public Task ReportAsync(string e164, ReportVerdictDto verdict, string? label)
    {
        var cleanLabel = verdict == ReportVerdictDto.Spam && !string.IsNullOrWhiteSpace(label) ? label.Trim() : null;
        myReports.Save(new MyReport(e164, verdict, cleanLabel, timeProvider.GetUtcNow()));

        return outbox.EnqueueAsync(PendingOperationKind.Report, e164, verdict, cleanLabel);
    }

    public Task WithdrawAsync(string e164)
    {
        myReports.Remove(e164);

        return outbox.EnqueueAsync(PendingOperationKind.WithdrawReport, e164);
    }
}
