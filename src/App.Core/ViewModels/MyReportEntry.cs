using CommunityToolkit.Mvvm.ComponentModel;
using Tranqui.App.Core.Reports;
using Tranqui.App.Core.Resources;
using Tranqui.Contracts.Reports;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.ViewModels;

/// <summary>One row of "My reports". The full number is shown: it never leaves the phone in clear.</summary>
public sealed partial class MyReportEntry(MyReport report, bool isPending) : ObservableObject
{
    public MyReport Report { get; } = report;

    public string Number => PhoneNumber.TryParse(Report.E164)?.E164 ?? Report.E164;

    public string VerdictText => Report.Verdict switch
    {
        ReportVerdictDto.Spam when Report.Label is { } label => Texts.Format(Texts.MyReportSpamWithLabelFormat, label),
        ReportVerdictDto.Spam => Texts.MyReportSpam,
        _ => Texts.MyReportNotSpam,
    };

    public string ReportedAtText => Report.ReportedAt.LocalDateTime.ToString("g", System.Globalization.CultureInfo.CurrentCulture);

    public bool IsPending { get; } = isPending;
}
