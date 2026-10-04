using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tranqui.App.Core.Dialogs;
using Tranqui.App.Core.Reports;
using Tranqui.App.Core.Resources;
using Tranqui.App.Core.Sync;
using Tranqui.Contracts.Reports;
using Tranqui.Domain.Reputation;

namespace Tranqui.App.Core.ViewModels;

/// <summary>The user's own reports (kept on the phone): change the verdict or label, or withdraw them.</summary>
public sealed partial class MyReportsViewModel(
    IMyReports myReports,
    ReportService reports,
    Outbox outbox,
    IDialogService dialogs) : FormViewModel
{
    public ObservableCollection<MyReportEntry> Entries { get; } = [];

    [ObservableProperty]
    public partial bool IsEmpty { get; set; } = true;

    [RelayCommand]
    private void Load()
    {
        var pending = outbox.Pending
            .Where(operation => operation.Kind == PendingOperationKind.Report)
            .Select(operation => operation.E164)
            .ToHashSet(StringComparer.Ordinal);

        Entries.Clear();
        foreach (var report in myReports.List().OrderByDescending(report => report.ReportedAt))
        {
            Entries.Add(new MyReportEntry(report, pending.Contains(report.E164)));
        }

        IsEmpty = Entries.Count == 0;
    }

    [RelayCommand]
    private async Task ChangeToSpamAsync(MyReportEntry entry)
    {
        var label = await dialogs.PromptAsync(
            Texts.SpamLabelTitle, Texts.SpamLabelMessage, Texts.SpamLabelPlaceholder, CallerName.MaxLength);
        if (label is null)
        {
            return;
        }

        await reports.ReportAsync(entry.Report.E164, ReportVerdictDto.Spam, label);
        InfoMessage = Texts.ReportQueued;
        Load();
    }

    [RelayCommand]
    private async Task ChangeToNotSpamAsync(MyReportEntry entry)
    {
        await reports.ReportAsync(entry.Report.E164, ReportVerdictDto.NotSpam, label: null);
        InfoMessage = Texts.ReportQueued;
        Load();
    }

    [RelayCommand]
    private async Task WithdrawAsync(MyReportEntry entry)
    {
        if (!await dialogs.ConfirmAsync(Texts.WithdrawReportTitle, Texts.WithdrawReportMessage, Texts.WithdrawReport, Texts.Cancel))
        {
            return;
        }

        await reports.WithdrawAsync(entry.Report.E164);
        InfoMessage = Texts.ReportWithdrawn;
        Load();
    }
}
