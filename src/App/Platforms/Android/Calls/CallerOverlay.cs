using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Resources;

namespace Tranqui.App.Calls;

/// <summary>
/// The colored card shown on top of the incoming-call screen. Built with native views (not MAUI) so it can appear
/// while the app is in the background. Shows the main name, lets the user slide through the others, and block.
/// </summary>
internal sealed class CallerOverlay(Context context)
{
    private static readonly TimeSpan visibleFor = TimeSpan.FromSeconds(45);
    private const int PaddingDp = 16;
    private const float TitleSizeSp = 22;
    private const float SubtitleSizeSp = 15;

    private readonly IWindowManager? windowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
    private AView? view;

    public bool CanShow => Settings.CanDrawOverlays(context) && windowManager is not null;

    public void Show(CallerCard card, Action onBlock)
    {
        Dismiss();
        var nameIndex = 0;

        var title = Text($"{CallerCardStyle.Symbol(card.State)} {card.Title}", TitleSizeSp, bold: true);
        var subtitle = Text(card.Subtitle, SubtitleSizeSp, bold: false);
        var layout = new LinearLayout(context) { Orientation = global::Android.Widget.Orientation.Vertical };
        var padding = Dp(PaddingDp);
        layout.SetPadding(padding, padding, padding, padding);
        layout.SetBackgroundColor(AColor.ParseColor(CallerCardStyle.BackgroundHex(card.State)));
        layout.ContentDescription = $"{card.Title}. {card.Subtitle}";
        layout.AddView(title);
        layout.AddView(subtitle);

        var actions = new LinearLayout(context) { Orientation = global::Android.Widget.Orientation.Horizontal };
        if (card.Names.Count > 1)
        {
            actions.AddView(Button(Texts.OverlayAnotherName, () =>
            {
                nameIndex = (nameIndex + 1) % card.Names.Count;
                title.Text = $"{CallerCardStyle.Symbol(card.State)} {card.Names[nameIndex]}";
            }));
        }

        if (card.Number is not null && card.State != CallerCardState.KnownContact)
        {
            actions.AddView(Button(Texts.Block, () =>
            {
                onBlock();
                subtitle.Text = Texts.NumberBlocked;
            }));
        }

        actions.AddView(Button(Texts.Close, Dismiss));
        layout.AddView(actions);

        var parameters = new WindowManagerLayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.WrapContent,
            WindowManagerTypes.ApplicationOverlay,
            WindowManagerFlags.NotFocusable | WindowManagerFlags.LayoutInScreen,
            Format.Translucent)
        {
            Gravity = GravityFlags.Top,
        };

        windowManager!.AddView(layout, parameters);
        view = layout;
        new Handler(Looper.MainLooper!).PostDelayed(Dismiss, (long)visibleFor.TotalMilliseconds);
    }

    public void Dismiss()
    {
        if (view is not null)
        {
            windowManager?.RemoveView(view);
            view = null;
        }
    }

    private TextView Text(string value, float sizeSp, bool bold)
    {
        var text = new TextView(context) { Text = value, TextSize = sizeSp };
        text.SetTextColor(AColor.White);
        if (bold)
        {
            text.SetTypeface(text.Typeface, TypefaceStyle.Bold);
        }

        return text;
    }

    private global::Android.Widget.Button Button(string label, Action onClick)
    {
        var button = new global::Android.Widget.Button(context) { Text = label };
        button.Click += (_, _) => onClick();
        return button;
    }

    private int Dp(int value) => (int)(value * (context.Resources?.DisplayMetrics?.Density ?? 1));
}
