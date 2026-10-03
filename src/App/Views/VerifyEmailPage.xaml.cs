using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Views;

public partial class VerifyEmailPage : ContentPage
{
    public VerifyEmailPage(VerifyEmailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
