using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Resources;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.History;

/// <summary>A history row: the call plus a one-line, human summary of what happened and what the user did.</summary>
public sealed partial class CallHistoryEntry(CallRecord record) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Summary), nameof(CanReport))]
    public partial CallRecord Record { get; set; } = record;

    public string Title => Record.Title;

    public string When => Record.OccurredAt.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);

    public bool CanReport => Record.E164 is not null && Record.MyVerdict is null && Record.State != CallerCardState.KnownContact;

    public string Summary
    {
        get
        {
            var what = Record.BlockReason is null
                ? StateText(Record.State)
                : Texts.Format(Texts.HistoryBlockedFormat, BlockText(Record.BlockReason.Value));
            return Record.MyVerdict switch
            {
                ReportVerdictDto.Spam => Texts.Format(Texts.HistoryReportedSpamFormat, what),
                ReportVerdictDto.NotSpam => Texts.Format(Texts.HistoryReportedNotSpamFormat, what),
                _ => what,
            };
        }
    }

    private static string StateText(CallerCardState state) => state switch
    {
        CallerCardState.KnownContact => Texts.HistoryKnownContact,
        CallerCardState.Identified => Texts.HistoryIdentified,
        CallerCardState.Spam => Texts.HistoryPossibleSpam,
        CallerCardState.Offline => Texts.HistoryOffline,
        CallerCardState.PrivateNumber => Texts.HistoryPrivateNumber,
        _ => Texts.HistoryUnknown,
    };

    private static string BlockText(BlockReason reason) => reason switch
    {
        BlockReason.BlockedByUser => Texts.BlockReasonByUser,
        BlockReason.CommunitySpam => Texts.BlockReasonCommunitySpam,
        BlockReason.PrivateNumber => Texts.BlockReasonPrivate,
        _ => Texts.BlockReasonInternational,
    };
}
