using Tranqui.App.Core.Appearance;
using Tranqui.App.Core.Sync;

namespace Tranqui.App;

public partial class App : Application
{
    private readonly Outbox outbox;

    public App(IThemeService themes, Outbox outbox)
    {
        InitializeComponent();
        this.outbox = outbox;
        themes.Apply(themes.Current);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    /// <summary>Opening the app is a good moment to send anything that was left pending while offline.</summary>
    protected override void OnStart() => _ = outbox.FlushAsync(CancellationToken.None);

    protected override void OnResume() => _ = outbox.FlushAsync(CancellationToken.None);
}
