using Tranqui.App.Core.Navigation;

namespace Tranqui.App.Services;

internal sealed class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route) =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(route));
}
