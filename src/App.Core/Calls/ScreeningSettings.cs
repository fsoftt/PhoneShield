namespace Tranqui.App.Core.Calls;

/// <summary>Automatic blocking options chosen by the user in Settings.</summary>
public sealed record ScreeningSettings(bool BlockCommunitySpam, bool BlockPrivateNumbers, bool BlockInternational)
{
    public static ScreeningSettings Default { get; } = new(BlockCommunitySpam: false, BlockPrivateNumbers: false, BlockInternational: false);
}
