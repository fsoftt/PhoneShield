using Tranqui.App.Core.Onboarding;

namespace Tranqui.App.Services;

internal sealed class PreferencesOnboardingState : IOnboardingState
{
    private const string BlockSharingNoticeKey = "tranqui.notice.block_sharing";

    public bool BlockSharingNoticeSeen
    {
        get => Preferences.Default.Get(BlockSharingNoticeKey, false);
        set => Preferences.Default.Set(BlockSharingNoticeKey, value);
    }
}
