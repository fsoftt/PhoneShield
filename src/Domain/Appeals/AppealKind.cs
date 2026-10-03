namespace Tranqui.Domain.Appeals;

public enum AppealKind
{
    /// <summary>Stop showing any name for the number. Applied at once; does not change whether it is spam.</summary>
    HideNames = 1,

    /// <summary>The owner says the number is not spam. Reviewed by a person.</summary>
    ReviewSpam = 2,
}
