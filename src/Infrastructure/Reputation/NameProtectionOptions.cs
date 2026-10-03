namespace Tranqui.Infrastructure.Reputation;

public sealed class NameProtectionOptions
{
    public const string SectionName = "NameProtection";

    /// <summary>Base64-encoded master key for caller names. Secret: never commit it.</summary>
    public string Key { get; set; } = string.Empty;
}
