using System.Globalization;
using System.Resources;

namespace Tranqui.App.Core.Resources;

/// <summary>
/// User-facing text in the device language: Texts.resx is Spanish (the default and fallback), Texts.en.resx English.
/// Add a key to every Texts.*.resx file and a property here; LocalizationTests fails if a language misses a key.
/// </summary>
public static class Texts
{
    public static ResourceManager ResourceManager { get; } = new("Tranqui.App.Core.Resources.Texts", typeof(Texts).Assembly);

    public static string EmailRequired => Get(nameof(EmailRequired));

    public static string PasswordRequired => Get(nameof(PasswordRequired));

    public static string PasswordTooShort => Get(nameof(PasswordTooShort));

    public static string PasswordsDoNotMatch => Get(nameof(PasswordsDoNotMatch));

    public static string TermsRequired => Get(nameof(TermsRequired));

    public static string PasswordResetSent => Get(nameof(PasswordResetSent));

    public static string VerificationSent => Get(nameof(VerificationSent));

    public static string EmailNotVerifiedYet => Get(nameof(EmailNotVerifiedYet));

    public static string ConnectionError => Get(nameof(ConnectionError));

    public static string ErrorInvalidRequest => Get(nameof(ErrorInvalidRequest));

    public static string ErrorAccountNotRegistered => Get(nameof(ErrorAccountNotRegistered));

    public static string ErrorConsentRequired => Get(nameof(ErrorConsentRequired));

    public static string ErrorTooManyRequests => Get(nameof(ErrorTooManyRequests));

    public static string ErrorUnexpected => Get(nameof(ErrorUnexpected));

    public static string ErrorSessionExpired => Get(nameof(ErrorSessionExpired));

    public static string FirebaseEmailExists => Get(nameof(FirebaseEmailExists));

    public static string FirebaseInvalidEmail => Get(nameof(FirebaseInvalidEmail));

    public static string FirebaseWrongCredentials => Get(nameof(FirebaseWrongCredentials));

    public static string FirebaseUserDisabled => Get(nameof(FirebaseUserDisabled));

    public static string FirebaseTooManyAttempts => Get(nameof(FirebaseTooManyAttempts));

    public static string FirebaseSessionExpired => Get(nameof(FirebaseSessionExpired));

    public static string FirebaseSignInAgain => Get(nameof(FirebaseSignInAgain));

    public static string FirebaseUnknownError => Get(nameof(FirebaseUnknownError));

    public static string PrivateNumberTitle => Get(nameof(PrivateNumberTitle));

    public static string PrivateNumberSubtitle => Get(nameof(PrivateNumberSubtitle));

    public static string KnownContactSubtitle => Get(nameof(KnownContactSubtitle));

    public static string KnownContactButSpamSubtitle => Get(nameof(KnownContactButSpamSubtitle));

    public static string SpamTitle => Get(nameof(SpamTitle));

    public static string SpamSubtitleFormat => Get(nameof(SpamSubtitleFormat));

    public static string IdentifiedSubtitle => Get(nameof(IdentifiedSubtitle));

    public static string UnknownTitle => Get(nameof(UnknownTitle));

    public static string UnknownSubtitle => Get(nameof(UnknownSubtitle));

    public static string OfflineSubtitle => Get(nameof(OfflineSubtitle));

    public static string StepCallScreeningTitle => Get(nameof(StepCallScreeningTitle));

    public static string StepCallScreeningExplanation => Get(nameof(StepCallScreeningExplanation));

    public static string StepOverlayTitle => Get(nameof(StepOverlayTitle));

    public static string StepOverlayExplanation => Get(nameof(StepOverlayExplanation));

    public static string StepContactsTitle => Get(nameof(StepContactsTitle));

    public static string StepContactsExplanation => Get(nameof(StepContactsExplanation));

    public static string StepNotificationsTitle => Get(nameof(StepNotificationsTitle));

    public static string StepNotificationsExplanation => Get(nameof(StepNotificationsExplanation));

    public static string ProtectionActive => Get(nameof(ProtectionActive));

    public static string ProtectionIncomplete => Get(nameof(ProtectionIncomplete));

    public static string Done => Get(nameof(Done));

    public static string Enable => Get(nameof(Enable));

    public static string DeleteAccountTitle => Get(nameof(DeleteAccountTitle));

    public static string DeleteAccountMessage => Get(nameof(DeleteAccountMessage));

    public static string DeleteAccountAccept => Get(nameof(DeleteAccountAccept));

    public static string Cancel => Get(nameof(Cancel));

    public static string MyDataSummaryFormat => Get(nameof(MyDataSummaryFormat));

    public static string NoBlockedNumbers => Get(nameof(NoBlockedNumbers));

    public static string ContributionExplanation => Get(nameof(ContributionExplanation));

    public static string ContributionStartedFormat => Get(nameof(ContributionStartedFormat));

    public static string ContributionStopped => Get(nameof(ContributionStopped));

    public static string ContactsPermissionNeeded => Get(nameof(ContactsPermissionNeeded));

    public static string SpamLabelTitle => Get(nameof(SpamLabelTitle));

    public static string SpamLabelMessage => Get(nameof(SpamLabelMessage));

    public static string SpamLabelPlaceholder => Get(nameof(SpamLabelPlaceholder));

    public static string Send => Get(nameof(Send));

    public static string ReportSent => Get(nameof(ReportSent));

    public static string NumberBlocked => Get(nameof(NumberBlocked));

    public static string HistoryKnownContact => Get(nameof(HistoryKnownContact));

    public static string HistoryIdentified => Get(nameof(HistoryIdentified));

    public static string HistoryPossibleSpam => Get(nameof(HistoryPossibleSpam));

    public static string HistoryOffline => Get(nameof(HistoryOffline));

    public static string HistoryPrivateNumber => Get(nameof(HistoryPrivateNumber));

    public static string HistoryUnknown => Get(nameof(HistoryUnknown));

    public static string HistoryBlockedFormat => Get(nameof(HistoryBlockedFormat));

    public static string HistoryReportedSpamFormat => Get(nameof(HistoryReportedSpamFormat));

    public static string HistoryReportedNotSpamFormat => Get(nameof(HistoryReportedNotSpamFormat));

    public static string BlockReasonByUser => Get(nameof(BlockReasonByUser));

    public static string BlockReasonCommunitySpam => Get(nameof(BlockReasonCommunitySpam));

    public static string BlockReasonPrivate => Get(nameof(BlockReasonPrivate));

    public static string BlockReasonInternational => Get(nameof(BlockReasonInternational));

    public static string ChannelIncoming => Get(nameof(ChannelIncoming));

    public static string ChannelBlocked => Get(nameof(ChannelBlocked));

    public static string ChannelFeedback => Get(nameof(ChannelFeedback));

    public static string BlockedCallTitleFormat => Get(nameof(BlockedCallTitleFormat));

    public static string BlockedBecauseYouBlocked => Get(nameof(BlockedBecauseYouBlocked));

    public static string BlockedBecauseSpamFormat => Get(nameof(BlockedBecauseSpamFormat));

    public static string BlockedBecausePrivate => Get(nameof(BlockedBecausePrivate));

    public static string BlockedBecauseInternational => Get(nameof(BlockedBecauseInternational));

    public static string FeedbackTitle => Get(nameof(FeedbackTitle));

    public static string FeedbackTextFormat => Get(nameof(FeedbackTextFormat));

    public static string FeedbackSpam => Get(nameof(FeedbackSpam));

    public static string FeedbackNotSpam => Get(nameof(FeedbackNotSpam));

    public static string OverlayAnotherName => Get(nameof(OverlayAnotherName));

    public static string Block => Get(nameof(Block));

    public static string Close => Get(nameof(Close));

    public static string Loading => Get(nameof(Loading));

    public static string AppTagline => Get(nameof(AppTagline));

    public static string EmailPlaceholder => Get(nameof(EmailPlaceholder));

    public static string EmailDescription => Get(nameof(EmailDescription));

    public static string PasswordPlaceholder => Get(nameof(PasswordPlaceholder));

    public static string SignInButton => Get(nameof(SignInButton));

    public static string ForgotPassword => Get(nameof(ForgotPassword));

    public static string CreateAccountLink => Get(nameof(CreateAccountLink));

    public static string SignUpTitle => Get(nameof(SignUpTitle));

    public static string SignUpHeading => Get(nameof(SignUpHeading));

    public static string PhoneNotLinked => Get(nameof(PhoneNotLinked));

    public static string PasswordMinPlaceholder => Get(nameof(PasswordMinPlaceholder));

    public static string ConfirmPasswordPlaceholder => Get(nameof(ConfirmPasswordPlaceholder));

    public static string AcceptTermsLabel => Get(nameof(AcceptTermsLabel));

    public static string VerifyEmailHeading => Get(nameof(VerifyEmailHeading));

    public static string VerifyEmailSentPrefix => Get(nameof(VerifyEmailSentPrefix));

    public static string VerifyEmailSentSuffix => Get(nameof(VerifyEmailSentSuffix));

    public static string AlreadyVerified => Get(nameof(AlreadyVerified));

    public static string ResendEmail => Get(nameof(ResendEmail));

    public static string UseAnotherAccount => Get(nameof(UseAnotherAccount));

    public static string TabHome => Get(nameof(TabHome));

    public static string TabHistory => Get(nameof(TabHistory));

    public static string TabBlocked => Get(nameof(TabBlocked));

    public static string TabSettings => Get(nameof(TabSettings));

    public static string TabAccount => Get(nameof(TabAccount));

    public static string HistoryHeading => Get(nameof(HistoryHeading));

    public static string HistoryRetentionNote => Get(nameof(HistoryRetentionNote));

    public static string HistoryEmpty => Get(nameof(HistoryEmpty));

    public static string ClearHistory => Get(nameof(ClearHistory));

    public static string BlockedHeading => Get(nameof(BlockedHeading));

    public static string Unblock => Get(nameof(Unblock));

    public static string AutoBlockHeading => Get(nameof(AutoBlockHeading));

    public static string AutoBlockIntro => Get(nameof(AutoBlockIntro));

    public static string BlockSpamTitle => Get(nameof(BlockSpamTitle));

    public static string BlockSpamDetail => Get(nameof(BlockSpamDetail));

    public static string BlockPrivateTitle => Get(nameof(BlockPrivateTitle));

    public static string BlockPrivateDetail => Get(nameof(BlockPrivateDetail));

    public static string BlockInternationalTitle => Get(nameof(BlockInternationalTitle));

    public static string BlockInternationalDetail => Get(nameof(BlockInternationalDetail));

    public static string ContributeHeading => Get(nameof(ContributeHeading));

    public static string StartContributing => Get(nameof(StartContributing));

    public static string StopContributing => Get(nameof(StopContributing));

    public static string AccountHeading => Get(nameof(AccountHeading));

    public static string AccountPrivacyNote => Get(nameof(AccountPrivacyNote));

    public static string ShowMyData => Get(nameof(ShowMyData));

    public static string SignOut => Get(nameof(SignOut));

    public static string DeleteMyAccount => Get(nameof(DeleteMyAccount));

    /// <summary>Formats a localized template such as <see cref="SpamSubtitleFormat"/> with the current culture.</summary>
    public static string Format(string template, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, template, arguments);

    private static string Get(string name) => ResourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;
}
