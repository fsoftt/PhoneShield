using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Tranqui.App.Core.Calls;
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
            var what = Record.BlockReason is null ? StateText(Record.State) : $"Bloqueada · {BlockText(Record.BlockReason.Value)}";
            return Record.MyVerdict switch
            {
                ReportVerdictDto.Spam => $"{what} · Reportaste spam",
                ReportVerdictDto.NotSpam => $"{what} · Dijiste que no es spam",
                _ => what,
            };
        }
    }

    private static string StateText(CallerCardState state) => state switch
    {
        CallerCardState.KnownContact => "En tus contactos",
        CallerCardState.Identified => "Identificado por la comunidad",
        CallerCardState.Spam => "Posible spam",
        CallerCardState.Offline => "Sin conexión al recibirla",
        CallerCardState.PrivateNumber => "Número privado",
        _ => "Número desconocido",
    };

    private static string BlockText(BlockReason reason) => reason switch
    {
        BlockReason.BlockedByUser => "lo bloqueaste tú",
        BlockReason.CommunitySpam => "spam de la comunidad",
        BlockReason.PrivateNumber => "número privado",
        _ => "internacional",
    };
}
