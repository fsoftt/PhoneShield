using System.Globalization;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Tranqui.App.Core.Calls;
using Tranqui.Domain.PhoneNumbers;
using AColor = Android.Graphics.Color;
using AndroidResource = Android.Resource;

namespace Tranqui.App.Calls;

/// <summary>
/// Notifications about calls: "we blocked a call" with the reason, the caller card when the popup cannot be shown,
/// and the post-call "¿Cómo fue esta llamada?" with Spam / No es spam actions.
/// </summary>
internal sealed class CallNotifications(Context context)
{
    public const string FeedbackChannelId = "call_feedback";
    public const string ExtraNumber = "tranqui.number";
    public const string ExtraNotificationId = "tranqui.notification_id";

    private const string BlockedChannelId = "blocked_calls";
    private const string IncomingChannelId = "incoming_calls";

    private readonly NotificationManager? manager = context.GetSystemService(Context.NotificationService) as NotificationManager;

    public static void CreateChannels(Context context)
    {
        if (context.GetSystemService(Context.NotificationService) is not NotificationManager manager)
        {
            return;
        }

        manager.CreateNotificationChannel(new NotificationChannel(IncomingChannelId, "Llamadas entrantes", NotificationImportance.High));
        manager.CreateNotificationChannel(new NotificationChannel(BlockedChannelId, "Llamadas bloqueadas", NotificationImportance.Default));
        manager.CreateNotificationChannel(new NotificationChannel(FeedbackChannelId, "¿Cómo fue la llamada?", NotificationImportance.Low));
    }

    public void ShowBlocked(ScreeningDecision decision)
    {
        var title = $"Bloqueamos una llamada de {decision.Card.Number?.Masked ?? decision.Card.Title}";
        var reason = decision.BlockReason switch
        {
            BlockReason.BlockedByUser => "Tú bloqueaste este número.",
            BlockReason.CommunitySpam => $"La comunidad lo reportó como spam: {decision.Card.Title}.",
            BlockReason.PrivateNumber => "Bloqueas los números privados.",
            BlockReason.International => "Bloqueas las llamadas internacionales.",
            _ => decision.Card.Subtitle,
        };

        var time = DateTime.Now.ToString("t", CultureInfo.CurrentCulture);
        Notify(NewBuilder(BlockedChannelId, title, $"{reason} {time}").Build());
    }

    public void ShowCallerCard(CallerCard card) =>
        Notify(NewBuilder(IncomingChannelId, $"{CallerCardStyle.Symbol(card.State)} {card.Title}", card.Subtitle)
            .SetColor(AColor.ParseColor(CallerCardStyle.BackgroundHex(card.State)).ToArgb())
            .SetCategory(Notification.CategoryCall)
            .Build());

    public void AskForFeedback(PhoneNumber number)
    {
        var id = NewId();
        var builder = NewBuilder(FeedbackChannelId, "¿Cómo fue esta llamada?", $"Del número {number.Masked}. Tu respuesta ayuda a otros.")
            .AddAction(FeedbackAction(CallFeedbackReceiver.ActionSpam, "Es spam", number, id, requestCode: id * 2))
            .AddAction(FeedbackAction(CallFeedbackReceiver.ActionNotSpam, "No es spam", number, id, requestCode: (id * 2) + 1));

        manager?.Notify(id, builder.Build());
    }

    public void Cancel(int id) => manager?.Cancel(id);

    private Notification.Builder NewBuilder(string channelId, string title, string text) =>
        new Notification.Builder(context, channelId)
            .SetSmallIcon(AndroidResource.Drawable.SymCallIncoming)
            .SetContentTitle(title)
            .SetContentText(text)
            .SetAutoCancel(true);

    private Notification.Action FeedbackAction(string action, string label, PhoneNumber number, int notificationId, int requestCode)
    {
        var intent = new Intent(context, typeof(CallFeedbackReceiver)).SetAction(action);
        intent.PutExtra(ExtraNumber, number.E164);
        intent.PutExtra(ExtraNotificationId, notificationId);
        var pending = PendingIntent.GetBroadcast(
            context, requestCode, intent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        return new Notification.Action.Builder(
            Icon.CreateWithResource(context, AndroidResource.Drawable.SymCallIncoming), label, pending).Build();
    }

    private void Notify(Notification notification) => manager?.Notify(NewId(), notification);

    /// <summary>Halved so that <c>id * 2 + 1</c> (the action request codes) cannot overflow.</summary>
    private static int NewId() => (Environment.TickCount & int.MaxValue) / 2;
}
