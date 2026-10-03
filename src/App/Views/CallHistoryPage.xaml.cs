using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Views;

public partial class CallHistoryPage : ContentPage
{
    private readonly CallHistoryViewModel viewModel;

    public CallHistoryPage(CallHistoryViewModel viewModel)
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
