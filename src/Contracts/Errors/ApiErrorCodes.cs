namespace Tranqui.Contracts.Errors;

/// <summary>
/// Stable error codes in the "code" field of every API problem response. The API speaks English only;
/// clients show their own localized message for each code instead of the English title.
/// </summary>
public static class ApiErrorCodes
{
    public const string Field = "code";

    public const string ValidationFailed = "validation_failed";
    public const string AccountNotRegistered = "account_not_registered";
    public const string ConsentRequired = "consent_required";
    public const string RateLimited = "rate_limited";
    public const string AppealLimitReached = "appeal_limit_reached";
    public const string AppealAccountTooNew = "appeal_account_too_new";
    public const string DeviceNotTrusted = "device_not_trusted";
    public const string PhoneNotVerified = "phone_not_verified";
    public const string UnexpectedError = "unexpected_error";
}
