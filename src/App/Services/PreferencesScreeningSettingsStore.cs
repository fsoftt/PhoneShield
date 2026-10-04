using Tranqui.App.Core.Calls;

namespace Tranqui.App.Services;

internal sealed class PreferencesScreeningSettingsStore : IScreeningSettingsStore
{
    private const string BlockCommunitySpamKey = "tranqui.block_community_spam";
    private const string BlockPrivateNumbersKey = "tranqui.block_private_numbers";
    private const string BlockInternationalKey = "tranqui.block_international";
    private const string ShareBlocksKey = "tranqui.share_blocks";

    private readonly JsonPreferences<List<string>> prefixes = new("tranqui.blocked_prefixes", () => []);

    public ScreeningSettings Load() => new(
        Preferences.Default.Get(BlockCommunitySpamKey, ScreeningSettings.Default.BlockCommunitySpam),
        Preferences.Default.Get(BlockPrivateNumbersKey, ScreeningSettings.Default.BlockPrivateNumbers),
        Preferences.Default.Get(BlockInternationalKey, ScreeningSettings.Default.BlockInternational),
        prefixes.Read(),
        Preferences.Default.Get(ShareBlocksKey, ScreeningSettings.Default.ShareBlocks));

    public void Save(ScreeningSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Preferences.Default.Set(BlockCommunitySpamKey, settings.BlockCommunitySpam);
        Preferences.Default.Set(BlockPrivateNumbersKey, settings.BlockPrivateNumbers);
        Preferences.Default.Set(BlockInternationalKey, settings.BlockInternational);
        Preferences.Default.Set(ShareBlocksKey, settings.ShareBlocks);
        prefixes.Write([.. settings.BlockedPrefixes]);
    }
}
