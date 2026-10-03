namespace Tranqui.Domain.Appeals;

public enum QuotaSubjectKind
{
    /// <summary>The number's hash.</summary>
    Number = 1,

    /// <summary>The account's contributor id.</summary>
    Account = 2,

    /// <summary>The device's keyed hash.</summary>
    Device = 3,
}
