using Tranqui.App.Core.Dialogs;
using Tranqui.App.Core.Resources;

namespace Tranqui.App.Services;

internal sealed class ShellDialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel) =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlertAsync(title, message, accept, cancel));

    public Task<string?> PromptAsync(string title, string message, string placeholder, int maxLength) =>
        MainThread.InvokeOnMainThreadAsync<string?>(async () =>
            await Shell.Current.DisplayPromptAsync(title, message, Texts.Send, Texts.Cancel, placeholder, maxLength));
}
