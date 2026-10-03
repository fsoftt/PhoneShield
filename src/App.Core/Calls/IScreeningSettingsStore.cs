namespace Tranqui.App.Core.Calls;

public interface IScreeningSettingsStore
{
    ScreeningSettings Load();

    void Save(ScreeningSettings settings);
}
