using Android.App;
using Android.Content;
using Microsoft.Extensions.DependencyInjection;
using Tranqui.App.Core.Api;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Calls;

/// <summary>Sends the answer to "¿Cómo fue esta llamada?" as a report, then removes the notification.</summary>
[BroadcastReceiver(Exported = false)]
public sealed class CallFeedbackReceiver : BroadcastReceiver
{
    public const string ActionSpam = "tranqui.action.REPORT_SPAM";
    public const string ActionNotSpam = "tranqui.action.REPORT_NOT_SPAM";

    public override void OnReceive(Context? context, Intent? intent)
    {
        var number = intent?.GetStringExtra(CallNotifications.ExtraNumber);
        var verdict = intent?.Action switch
        {
            ActionSpam => ReportVerdictDto.Spam,
            ActionNotSpam => ReportVerdictDto.NotSpam,
            _ => (ReportVerdictDto?)null,
        };
        if (context is null || number is null || verdict is null)
        {
            return;
        }

        new CallNotifications(context).Cancel(intent!.GetIntExtra(CallNotifications.ExtraNotificationId, 0));

        var pending = GoAsync();
        _ = ReportAsync(number, verdict.Value, pending);
    }

    private static async Task ReportAsync(string number, ReportVerdictDto verdict, PendingResult? pending)
    {
        try
        {
            var api = IPlatformApplication.Current?.Services.GetRequiredService<ITranquiApi>();
            if (api is not null)
            {
                await api.ReportCallAsync(new ReportCallRequest(number, verdict, Label: null), CancellationToken.None);
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or Refit.ApiException or TaskCanceledException)
        {
            // Best effort: an offline report is dropped rather than retried with the number kept around.
        }
        finally
        {
            pending?.Finish();
        }
    }
}
