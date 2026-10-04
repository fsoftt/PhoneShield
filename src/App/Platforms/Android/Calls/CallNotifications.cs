using System.Globalization;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Resources;
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
    public const string LabelInputKey = "tranqui.label";

    private const string BlockedChannelId = "blocked_calls";
    private const string IncomingChannelId = "incoming_calls";

    private readonly NotificationManager? manager = context.GetSystemService(Context.NotificationService) as NotificationManager;

    public static void CreateChannels(Context context)
    {
        if (context.GetSystemService(Context.NotificationService) is not NotificationManager manager)
        {
            return;
        }

        manager.CreateNotificationChannel(new NotificationChannel(IncomingChannelId, Texts.ChannelIncoming, NotificationImportance.High));
        manager.CreateNotificationChannel(new NotificationChannel(BlockedChannelId, Texts.ChannelBlocked, NotificationImportance.Default));
        manager.CreateNotificationChannel(new NotificationChannel(FeedbackChannelId, Texts.ChannelFeedback, NotificationImportance.Low));
    }

    public void ShowBlocked(ScreeningDecision decision)
    {
        var title = Texts.Format(Texts.BlockedCallTitleFormat, decision.Card.Number?.Masked ?? decision.Card.Title);
        var reason = decision.BlockReason switch
        {
            BlockReason.BlockedByUser => Texts.BlockedBecauseYouBlocked,
            BlockReason.CommunitySpam => Texts.Format(Texts.BlockedBecauseSpamFormat, decision.Card.Title),
            BlockReason.PrivateNumber => Texts.BlockedBecausePrivate,
            BlockReason.International => Texts.BlockedBecauseInternational,
            BlockReason.Prefix => Texts.BlockedBecausePrefix,
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

    /// <summary>Later, once the community lookup answered for a call that rang as "Sin conexión".</summary>
    public void ShowLateIdentification(CallerCard card) =>
        Notify(NewBuilder(
                IncomingChannelId,
                Texts.LateIdentificationTitle,
                Texts.Format(Texts.LateIdentificationTextFormat, $"{CallerCardStyle.Symbol(card.State)} {card.Title}", card.Subtitle))
            .SetColor(AColor.ParseColor(CallerCardStyle.BackgroundHex(card.State)).ToArgb())
            .Build());

    /// <summary>"Es spam", "Spam con etiqueta…" (typed right in the notification) and "No es spam".</summary>
    public void AskForFeedback(PhoneNumber number)
    {
        var id = NewId();
        var builder = NewBuilder(FeedbackChannelId, Texts.FeedbackTitle, Texts.Format(Texts.FeedbackTextFormat, number.Masked))
            .AddAction(FeedbackAction(CallFeedbackReceiver.ActionSpam, Texts.FeedbackSpam, number, id, requestCode: id * 3))
            .AddAction(LabelAction(number, id, requestCode: (id * 3) + 1))
            .AddAction(FeedbackAction(CallFeedbackReceiver.ActionNotSpam, Texts.FeedbackNotSpam, number, id, requestCode: (id * 3) + 2));

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

    /// <summary>Inline reply: the text the user types arrives in the intent (so the PendingIntent must be mutable).</summary>
    private Notification.Action LabelAction(PhoneNumber number, int notificationId, int requestCode)
    {
        var intent = new Intent(context, typeof(CallFeedbackReceiver)).SetAction(CallFeedbackReceiver.ActionSpamWithLabel);
        intent.PutExtra(ExtraNumber, number.E164);
        intent.PutExtra(ExtraNotificationId, notificationId);
        // Before Android 12 every PendingIntent is mutable; from 12 on it has to be asked for explicitly.
        var mutable = OperatingSystem.IsAndroidVersionAtLeast(31) ? PendingIntentFlags.Mutable : 0;
        var pending = PendingIntent.GetBroadcast(context, requestCode, intent, mutable | PendingIntentFlags.UpdateCurrent);
        var input = new RemoteInput.Builder(LabelInputKey).SetLabel(Texts.FeedbackLabelHint)!.Build()!;

        return new Notification.Action.Builder(
                Icon.CreateWithResource(context, AndroidResource.Drawable.SymCallIncoming), Texts.FeedbackSpamWithLabel, pending)
            .AddRemoteInput(input)!
            .Build()!;
    }

    private void Notify(Notification notification) => manager?.Notify(NewId(), notification);

    /// <summary>Divided by three so that <c>id * 3 + 2</c> (the action request codes) cannot overflow.</summary>
    private static int NewId() => (Environment.TickCount & int.MaxValue) / 3;
}
