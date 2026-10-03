using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Views;

public partial class AppealPage : ContentPage
{
    public AppealPage(AppealViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
