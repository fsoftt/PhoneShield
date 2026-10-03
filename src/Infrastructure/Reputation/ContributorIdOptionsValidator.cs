using Microsoft.Extensions.Options;
using Tranqui.Infrastructure.Security;

namespace Tranqui.Infrastructure.Reputation;

internal sealed class ContributorIdOptionsValidator : IValidateOptions<ContributorIdOptions>
{
    public ValidateOptionsResult Validate(string? name, ContributorIdOptions options)
    {
        var failure = SecretKeyValidation.Validate($"{ContributorIdOptions.SectionName}:Key", options.Key);

        return failure is null ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failure);
    }
}
