using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Views;

public partial class BlockedNumbersPage : ContentPage
{
    private readonly BlockedNumbersViewModel viewModel;

    public BlockedNumbersPage(BlockedNumbersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadCommand.ExecuteAsync(null);
    }
}
