using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Views;

public partial class MyReportsPage : ContentPage
{
    private readonly MyReportsViewModel viewModel;

    public MyReportsPage(MyReportsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.LoadCommand.Execute(null);
    }
}
