using Tranqui.App.Core.Calls;

namespace Tranqui.App.Calls;

/// <summary>Color and symbol per popup state. The symbol and the title text carry the meaning; color only reinforces it.</summary>
internal static class CallerCardStyle
{
    public static string BackgroundHex(CallerCardState state) => state switch
    {
        CallerCardState.KnownContact => "#1E7B34",
        CallerCardState.Identified => "#1D5FB8",
        CallerCardState.Spam => "#C0262D",
        _ => "#B45309",
    };

    public static string Symbol(CallerCardState state) => state switch
    {
        CallerCardState.KnownContact => "✓",
        CallerCardState.Identified => "ⓘ",
        CallerCardState.Spam => "⚠",
        _ => "?",
    };
}
