namespace Tranqui.Infrastructure.Reputation;

public sealed class ContributorIdOptions
{
    public const string SectionName = "ContributorIds";

    /// <summary>Base64-encoded key that turns Firebase uids into contributor ids. Secret: never commit it.</summary>
    public string Key { get; set; } = string.Empty;
}
