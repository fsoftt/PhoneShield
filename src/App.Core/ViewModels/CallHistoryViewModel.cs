using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Dialogs;
using Tranqui.App.Core.History;
using Tranqui.App.Core.Reports;
using Tranqui.App.Core.Resources;
using Tranqui.Contracts.Reports;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.App.Core.ViewModels;

/// <summary>Recent screened calls (device only). From here the user reports spam with an optional label, or blocks.</summary>
public sealed partial class CallHistoryViewModel(
    ICallHistory history,
    ReportService reports,
    BlockingService blocking,
    IDialogService dialogs) : FormViewModel
{
    public ObservableCollection<CallHistoryEntry> Entries { get; } = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        Entries.Clear();
        foreach (var record in await history.ListAsync())
        {
            Entries.Add(new CallHistoryEntry(record));
        }
    }

    [RelayCommand]
    private async Task ReportSpamAsync(CallHistoryEntry entry)
    {
        var label = await dialogs.PromptAsync(
            Texts.SpamLabelTitle, Texts.SpamLabelMessage, Texts.SpamLabelPlaceholder, CallerName.MaxLength);
        if (label is null)
        {
            return;
        }

        await ReportAsync(entry, ReportVerdictDto.Spam, string.IsNullOrWhiteSpace(label) ? null : label.Trim());
    }

    [RelayCommand]
    private Task ReportNotSpamAsync(CallHistoryEntry entry) => ReportAsync(entry, ReportVerdictDto.NotSpam, label: null);

    [RelayCommand]
    private async Task BlockAsync(CallHistoryEntry entry)
    {
        if (PhoneNumber.TryParse(entry.Record.E164) is { } number)
        {
            await blocking.BlockAsync(number);
            InfoMessage = Texts.NumberBlocked;
        }
    }

    [RelayCommand]
    private async Task ClearAsync()
    {
        await history.ClearAsync();
        Entries.Clear();
    }

    /// <summary>Saved on the phone first and sent when there is a connection, so it never fails for being offline.</summary>
    private async Task ReportAsync(CallHistoryEntry entry, ReportVerdictDto verdict, string? label)
    {
        await reports.ReportAsync(entry.Record.E164!, verdict, label);
        await history.SetVerdictAsync(entry.Record.Id, verdict);
        entry.Record = entry.Record with { MyVerdict = verdict };
        InfoMessage = Texts.ReportQueued;
    }
}
