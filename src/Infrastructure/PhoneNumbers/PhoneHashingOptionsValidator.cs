using Microsoft.Extensions.Options;

namespace Tranqui.Infrastructure.PhoneNumbers;

/// <summary>Fails startup when hashing keys are missing or weak. Messages never include key material.</summary>
internal sealed class PhoneHashingOptionsValidator : IValidateOptions<PhoneHashingOptions>
{
    public const int MinimumKeySizeInBytes = 32;

    public ValidateOptionsResult Validate(string? name, PhoneHashingOptions options)
    {
        if (options.CurrentKeyVersion < 1)
        {
            return ValidateOptionsResult.Fail($"{PhoneHashingOptions.SectionName}:CurrentKeyVersion must be 1 or greater.");
        }

        var failures = Enumerable.Range(1, options.CurrentKeyVersion)
            .Select(version => ValidateKey(version, options.Keys.GetValueOrDefault(version)))
            .OfType<string>()
            .ToList();

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static string? ValidateKey(int version, string? base64Key)
    {
        var keyPath = $"{PhoneHashingOptions.SectionName}:Keys:{version}";

        if (string.IsNullOrWhiteSpace(base64Key))
        {
            return $"{keyPath} is not configured.";
        }

        var buffer = new byte[base64Key.Length];
        if (!Convert.TryFromBase64String(base64Key, buffer, out var keyLength))
        {
            return $"{keyPath} is not valid Base64.";
        }

        return keyLength < MinimumKeySizeInBytes
            ? $"{keyPath} must be at least {MinimumKeySizeInBytes} bytes."
            : null;
    }
}
