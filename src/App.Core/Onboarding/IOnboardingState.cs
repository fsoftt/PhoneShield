namespace Tranqui.App.Core.Onboarding;

/// <summary>One-time notices the user has already seen. Provided by the platform (app preferences).</summary>
public interface IOnboardingState
{
    bool BlockSharingNoticeSeen { get; set; }
}
