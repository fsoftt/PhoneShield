using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Views;

public partial class AccountPage : ContentPage
{
    public AccountPage(AccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
