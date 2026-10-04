using CoreTheme = Tranqui.App.Core.Appearance.AppTheme;
using IThemeService = Tranqui.App.Core.Appearance.IThemeService;

namespace Tranqui.App.Services;

/// <summary>Saves the chosen theme and applies it to the whole app.</summary>
internal sealed class MauiThemeService : IThemeService
{
    private const string Key = "tranqui.theme";

    public CoreTheme Current =>
        Enum.TryParse<CoreTheme>(Preferences.Default.Get(Key, nameof(CoreTheme.System)), out var theme) ? theme : CoreTheme.System;

    public void Apply(CoreTheme theme)
    {
        Preferences.Default.Set(Key, theme.ToString());
        if (Application.Current is { } application)
        {
            application.UserAppTheme = theme switch
            {
                CoreTheme.Light => AppTheme.Light,
                CoreTheme.Dark => AppTheme.Dark,
                _ => AppTheme.Unspecified,
            };
        }
    }
}
