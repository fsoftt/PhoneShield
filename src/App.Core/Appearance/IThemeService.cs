namespace Tranqui.App.Core.Appearance;

/// <summary>Remembers and applies the chosen theme. Provided by the platform.</summary>
public interface IThemeService
{
    AppTheme Current { get; }

    void Apply(AppTheme theme);
}
