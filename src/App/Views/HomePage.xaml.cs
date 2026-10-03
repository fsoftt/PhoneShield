using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Views;

/// <summary>Permissions are granted in system screens, so the checklist refreshes every time the app comes back.</summary>
public partial class HomePage : ContentPage
{
    private readonly HomeViewModel viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.Refresh();
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (Window is not null)
        {
            Window.Resumed += OnResumed;
        }
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (Window is not null)
        {
            Window.Resumed -= OnResumed;
        }
    }

    private void OnResumed(object? sender, EventArgs e) => viewModel.Refresh();
}
