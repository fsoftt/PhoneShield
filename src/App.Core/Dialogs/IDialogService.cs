namespace Tranqui.App.Core.Dialogs;

public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);

    /// <summary>Asks for a short text; returns null when the user cancels.</summary>
    Task<string?> PromptAsync(string title, string message, string placeholder, int maxLength);
}
