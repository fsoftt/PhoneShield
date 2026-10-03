using Tranqui.App.Core.Navigation;
using Tranqui.App.Views;

namespace Tranqui.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(Routes.SignUp, typeof(SignUpPage));
    }
}
