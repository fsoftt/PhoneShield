using Microsoft.Extensions.Options;
using Tranqui.Infrastructure.Security;

namespace Tranqui.Infrastructure.Reputation;

internal sealed class NameProtectionOptionsValidator : IValidateOptions<NameProtectionOptions>
{
    public ValidateOptionsResult Validate(string? name, NameProtectionOptions options)
    {
        var failure = SecretKeyValidation.Validate($"{NameProtectionOptions.SectionName}:Key", options.Key);

        return failure is null ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failure);
    }
}
