using Microsoft.Extensions.Options;
using Tranqui.Infrastructure.Security;

namespace Tranqui.Infrastructure.PhoneNumbers;

/// <summary>Fails startup when hashing keys are missing or weak. Messages never include key material.</summary>
internal sealed class PhoneHashingOptionsValidator : IValidateOptions<PhoneHashingOptions>
{
    public ValidateOptionsResult Validate(string? name, PhoneHashingOptions options)
    {
        if (options.CurrentKeyVersion < 1)
        {
            return ValidateOptionsResult.Fail($"{PhoneHashingOptions.SectionName}:CurrentKeyVersion must be 1 or greater.");
        }

        var failures = Enumerable.Range(1, options.CurrentKeyVersion)
            .Select(version => SecretKeyValidation.Validate(
                $"{PhoneHashingOptions.SectionName}:Keys:{version}",
                options.Keys.GetValueOrDefault(version)))
            .OfType<string>()
            .ToList();

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
