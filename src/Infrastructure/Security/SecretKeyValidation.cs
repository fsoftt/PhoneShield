namespace Tranqui.Infrastructure.Security;

/// <summary>Shared checks for Base64-encoded secret keys. Messages name the setting, never the key material.</summary>
internal static class SecretKeyValidation
{
    public const int MinimumKeySizeInBytes = 32;

    public static string? Validate(string settingPath, string? base64Key)
    {
        if (string.IsNullOrWhiteSpace(base64Key))
        {
            return $"{settingPath} is not configured.";
        }

        var buffer = new byte[base64Key.Length];
        if (!Convert.TryFromBase64String(base64Key, buffer, out var keyLength))
        {
            return $"{settingPath} is not valid Base64.";
        }

        return keyLength < MinimumKeySizeInBytes
            ? $"{settingPath} must be at least {MinimumKeySizeInBytes} bytes."
            : null;
    }
}
