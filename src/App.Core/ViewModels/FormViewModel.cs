using CommunityToolkit.Mvvm.ComponentModel;
using Refit;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Resources;

namespace Tranqui.App.Core.ViewModels;

/// <summary>Busy state and user-facing error/info messages shared by the account forms.</summary>
public abstract partial class FormViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? InfoMessage { get; set; }

    public bool IsNotBusy => !IsBusy;

    protected async Task RunAsync(Func<Task> action)
    {
        IsBusy = true;
        ErrorMessage = null;
        InfoMessage = null;
        try
        {
            await action();
        }
        catch (FirebaseAuthException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (ApiException exception)
        {
            ErrorMessage = ApiErrorMessages.For(exception);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = Texts.ConnectionError;
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected bool Require(bool condition, string message)
    {
        if (!condition)
        {
            ErrorMessage = message;
        }

        return condition;
    }
}
