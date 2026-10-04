namespace Tranqui.Contracts.BackOffice;

public enum AppealDecisionDto
{
    /// <summary>The number is not spam: earlier spam reports stop counting.</summary>
    Approve = 1,

    /// <summary>The reports stand.</summary>
    Reject = 2,
}
