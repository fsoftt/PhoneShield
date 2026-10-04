using Tranqui.App.Core.Resources;

namespace Tranqui.App.Core.Appearance;

/// <summary>A theme with its label in the phone's language, for the picker in Settings.</summary>
public sealed record ThemeOption(AppTheme Theme, string Label)
{
    public static IReadOnlyList<ThemeOption> All { get; } =
    [
        new(AppTheme.System, Texts.ThemeSystem),
        new(AppTheme.Light, Texts.ThemeLight),
        new(AppTheme.Dark, Texts.ThemeDark),
    ];

    public override string ToString() => Label;
}
