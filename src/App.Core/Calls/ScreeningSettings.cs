namespace Tranqui.App.Core.Calls;

/// <summary>
/// Automatic blocking options chosen by the user in Settings. <see cref="BlockedPrefixes"/> are E.164 prefixes
/// (see <see cref="BlockedPrefix"/>); <see cref="ShareBlocks"/> lets the user's blocks count as a weak spam signal.
/// </summary>
public sealed record ScreeningSettings(
    bool BlockCommunitySpam,
    bool BlockPrivateNumbers,
    bool BlockInternational,
    IReadOnlyList<string> BlockedPrefixes,
    bool ShareBlocks)
{
    public static ScreeningSettings Default { get; } = new(
        BlockCommunitySpam: false,
        BlockPrivateNumbers: false,
        BlockInternational: false,
        BlockedPrefixes: [],
        ShareBlocks: true);

    public bool BlocksPrefixOf(string e164) =>
        BlockedPrefixes.Any(prefix => e164.StartsWith(prefix, StringComparison.Ordinal));
}
