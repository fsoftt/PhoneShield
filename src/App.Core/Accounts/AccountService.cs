using Tranqui.App.Core.Api;
using Tranqui.Contracts.Accounts;
using Tranqui.Domain.Legal;

namespace Tranqui.App.Core.Accounts;

internal sealed class AccountService(ITranquiApi api) : IAccountService
{
    public Task EnsureRegisteredAsync(CancellationToken cancellationToken) =>
        api.RegisterAccountAsync(new RegisterAccountRequest(LegalDocuments.CurrentTermsVersion), cancellationToken);
}
