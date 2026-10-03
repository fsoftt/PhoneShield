namespace Tranqui.App.Core.Navigation;

public interface INavigationService
{
    Task GoToAsync(string route);
}
