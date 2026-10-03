namespace Tranqui.Contracts.Appeals;

public enum AppealKindDto
{
    /// <summary>Stop showing community names for the number. Applied right away.</summary>
    HideNames = 1,

    /// <summary>The number is wrongly marked as spam. Reviewed by a person.</summary>
    ReviewSpam = 2,
}
