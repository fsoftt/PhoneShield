namespace Tranqui.Domain.Appeals;

/// <summary>Limits that keep SMS verification within the free tier and stop appeal spam.</summary>
public static class AppealRules
{
    /// <summary>Each number can ask for one verification SMS, and file one appeal, per window.</summary>
    public static readonly TimeSpan LimitWindow = TimeSpan.FromDays(30);

    public const int VerificationsPerWindow = 1;

    public const int AppealsPerWindow = 1;

    public const int ReasonMaxLength = 500;

    public const int ContactEmailMaxLength = 254;
}
