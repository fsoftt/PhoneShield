namespace Tranqui.App.Core.Accounts;

public interface IAccountService
{
    /// <summary>Registers the account with the current terms (accepted at sign-up). Idempotent on the server.</summary>
    Task EnsureRegisteredAsync(CancellationToken cancellationToken);
}
