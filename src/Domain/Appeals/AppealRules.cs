using Tranqui.Domain.Reputation;

namespace Tranqui.Domain.Appeals;

/// <summary>
/// Limits that keep SMS verification within the free tier and stop appeal abuse. Every SMS and every appeal is counted
/// against the number, the account and the device, so changing SIM cards or accounts does not reset them.
/// </summary>
public static class AppealRules
{
    public static readonly TimeSpan MonthWindow = TimeSpan.FromDays(30);

    public static readonly TimeSpan YearWindow = TimeSpan.FromDays(365);

    /// <summary>Per number, account and device: one SMS and one appeal per month.</summary>
    public const int UsesPerMonth = 1;

    /// <summary>Per account and device (a number's owner rarely needs more): three SMS and three appeals per year.</summary>
    public const int AccountOrDeviceUsesPerYear = 3;

    /// <summary>Same age at which an account's votes count in full: fresh throwaway accounts cannot appeal.</summary>
    public static readonly TimeSpan MinimumAccountAge = ReputationRules.NewAccountPeriod;

    /// <summary>A Play Integrity verdict older than this is a replay.</summary>
    public static readonly TimeSpan IntegrityVerdictMaxAge = TimeSpan.FromMinutes(5);

    /// <summary>The SMS sign-in that proves the number must be this recent.</summary>
    public static readonly TimeSpan PhoneProofMaxAge = TimeSpan.FromMinutes(15);

    /// <summary>Ley 1581 de 2012: complaints are answered within 15 business days.</summary>
    public const int ReviewDeadlineBusinessDays = 15;

    public const int ReasonMaxLength = 500;

    public const int ContactEmailMaxLength = 254;

    public const int IntegrityTokenMaxLength = 16_384;

    public const int PhoneProofMaxLength = 8_192;

    /// <summary>Whether a subject that used <paramref name="usedLastMonth"/> and <paramref name="usedLastYear"/> may use one more.</summary>
    public static bool Allows(QuotaSubjectKind kind, int usedLastMonth, int usedLastYear) =>
        usedLastMonth < UsesPerMonth
        && (kind == QuotaSubjectKind.Number || usedLastYear < AccountOrDeviceUsesPerYear);
}
