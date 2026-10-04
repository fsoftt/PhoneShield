namespace Tranqui.Domain.Appeals;

public enum AppealStatus
{
    /// <summary>A spam review waiting for a person.</summary>
    Pending = 1,

    /// <summary>Names hidden right away; nothing to review.</summary>
    Applied = 2,

    /// <summary>Spam review accepted: earlier spam reports no longer count.</summary>
    Approved = 3,

    /// <summary>Spam review declined: the reports stand.</summary>
    Rejected = 4,
}
