using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Views;

public partial class StartupPage : ContentPage
{
    private readonly StartupViewModel viewModel;
    private bool initialized;

    public StartupPage(StartupViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (initialized)
        {
            return;
        }

        initialized = true;
        await viewModel.InitializeAsync();
    }
}
