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

    public static string InvalidPhoneNumber => Get(nameof(InvalidPhoneNumber));

    public static string FirebaseInvalidCode => Get(nameof(FirebaseInvalidCode));

    public static string FirebaseCodeExpired => Get(nameof(FirebaseCodeExpired));

    public static string ErrorAppealLimitReached => Get(nameof(ErrorAppealLimitReached));

    public static string ErrorAppealAccountTooNew => Get(nameof(ErrorAppealAccountTooNew));

    public static string ErrorDeviceNotTrusted => Get(nameof(ErrorDeviceNotTrusted));

    public static string ErrorPhoneNotVerified => Get(nameof(ErrorPhoneNotVerified));

    public static string AppealOpen => Get(nameof(AppealOpen));

    public static string AppealTitle => Get(nameof(AppealTitle));

    public static string AppealIntro => Get(nameof(AppealIntro));

    public static string AppealPhoneLabel => Get(nameof(AppealPhoneLabel));

    public static string AppealKindHeading => Get(nameof(AppealKindHeading));

    public static string AppealHideNames => Get(nameof(AppealHideNames));

    public static string AppealReviewSpam => Get(nameof(AppealReviewSpam));

    public static string AppealReasonLabel => Get(nameof(AppealReasonLabel));

    public static string AppealEmailLabel => Get(nameof(AppealEmailLabel));

    public static string AppealEmailNote => Get(nameof(AppealEmailNote));

    public static string AppealLimitsNote => Get(nameof(AppealLimitsNote));

    public static string AppealSendCode => Get(nameof(AppealSendCode));

    public static string AppealCodeLabel => Get(nameof(AppealCodeLabel));

    public static string AppealCodeSentFormat => Get(nameof(AppealCodeSentFormat));

    public static string AppealCodeRequired => Get(nameof(AppealCodeRequired));

    public static string AppealSubmit => Get(nameof(AppealSubmit));

    public static string AppealApplied => Get(nameof(AppealApplied));

    public static string AppealPending => Get(nameof(AppealPending));

    public static string ThemeHeading => Get(nameof(ThemeHeading));

    public static string ThemeSystem => Get(nameof(ThemeSystem));

    public static string ThemeLight => Get(nameof(ThemeLight));

    public static string ThemeDark => Get(nameof(ThemeDark));

    public static string BlockReasonPrefix => Get(nameof(BlockReasonPrefix));

    public static string BlockedBecausePrefix => Get(nameof(BlockedBecausePrefix));

    public static string PrefixesHeading => Get(nameof(PrefixesHeading));

    public static string PrefixesHint => Get(nameof(PrefixesHint));

    public static string PrefixPlaceholder => Get(nameof(PrefixPlaceholder));

    public static string AddPrefix => Get(nameof(AddPrefix));

    public static string RemovePrefix => Get(nameof(RemovePrefix));

    public static string InvalidPrefix => Get(nameof(InvalidPrefix));

    public static string ShareBlocksTitle => Get(nameof(ShareBlocksTitle));

    public static string ShareBlocksDetail => Get(nameof(ShareBlocksDetail));

    public static string MyReportsOpen => Get(nameof(MyReportsOpen));

    public static string MyReportsEmpty => Get(nameof(MyReportsEmpty));

    public static string MyReportsHint => Get(nameof(MyReportsHint));

    public static string MyReportPending => Get(nameof(MyReportPending));

    public static string MyReportSpam => Get(nameof(MyReportSpam));

    public static string MyReportSpamWithLabelFormat => Get(nameof(MyReportSpamWithLabelFormat));

    public static string MyReportNotSpam => Get(nameof(MyReportNotSpam));

    public static string ChangeToSpam => Get(nameof(ChangeToSpam));

    public static string ChangeToNotSpam => Get(nameof(ChangeToNotSpam));

    public static string WithdrawReport => Get(nameof(WithdrawReport));

    public static string WithdrawReportTitle => Get(nameof(WithdrawReportTitle));

    public static string WithdrawReportMessage => Get(nameof(WithdrawReportMessage));

    public static string ReportWithdrawn => Get(nameof(ReportWithdrawn));

    public static string ReportQueued => Get(nameof(ReportQueued));

    public static string FeedbackSpamWithLabel => Get(nameof(FeedbackSpamWithLabel));

    public static string FeedbackLabelHint => Get(nameof(FeedbackLabelHint));

    public static string LateIdentificationTitle => Get(nameof(LateIdentificationTitle));

    public static string LateIdentificationTextFormat => Get(nameof(LateIdentificationTextFormat));

    public static string BlockSharingNoticeTitle => Get(nameof(BlockSharingNoticeTitle));

    public static string BlockSharingNoticeText => Get(nameof(BlockSharingNoticeText));

    public static string Understood => Get(nameof(Understood));

    public static string ChangeInSettings => Get(nameof(ChangeInSettings));

    public static string DeleteAlsoSharedBlocks => Get(nameof(DeleteAlsoSharedBlocks));

    /// <summary>Formats a localized template such as <see cref="SpamSubtitleFormat"/> with the current culture.</summary>
    public static string Format(string template, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, template, arguments);

    private static string Get(string name) => ResourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;
}
