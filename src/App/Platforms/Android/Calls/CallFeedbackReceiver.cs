using Android.App;
using Android.Content;
using Tranqui.App.Core.Reports;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Calls;

/// <summary>
/// Turns the answer to "¿Cómo fue esta llamada?" (optionally with a label typed in the notification) into a report.
/// The report is saved on the phone and sent when there is a connection, so answering offline is never lost.
/// </summary>
[BroadcastReceiver(Exported = false)]
public sealed class CallFeedbackReceiver : BroadcastReceiver
{
    public const string ActionSpam = "tranqui.action.REPORT_SPAM";
    public const string ActionSpamWithLabel = "tranqui.action.REPORT_SPAM_WITH_LABEL";
    public const string ActionNotSpam = "tranqui.action.REPORT_NOT_SPAM";

    public override void OnReceive(Context? context, Intent? intent)
    {
        var number = intent?.GetStringExtra(CallNotifications.ExtraNumber);
        var verdict = intent?.Action switch
        {
            ActionSpam or ActionSpamWithLabel => ReportVerdictDto.Spam,
            ActionNotSpam => ReportVerdictDto.NotSpam,
            _ => (ReportVerdictDto?)null,
        };
        if (context is null || number is null || verdict is null)
        {
            return;
        }

        var label = intent!.Action == ActionSpamWithLabel
            ? RemoteInput.GetResultsFromIntent(intent)?.GetCharSequence(CallNotifications.LabelInputKey)?.ToString()
            : null;
        new CallNotifications(context).Cancel(intent.GetIntExtra(CallNotifications.ExtraNotificationId, 0));

        var pending = GoAsync();
        _ = ReportAsync(number, verdict.Value, label, pending);
    }

    private static async Task ReportAsync(string number, ReportVerdictDto verdict, string? label, PendingResult? pending)
    {
        try
        {
            if (IPlatformApplication.Current?.Services.GetService<ReportService>() is { } reports)
            {
                await reports.ReportAsync(number, verdict, label);
            }
        }
        finally
        {
            pending?.Finish();
        }
    }
}
