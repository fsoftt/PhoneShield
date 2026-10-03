namespace Tranqui.App.Core.Calls;

/// <summary>
/// What to do with an incoming call. A rejected call never rings; the user gets a notification with the reason.
/// <see cref="AskForFeedback"/> asks "¿Cómo fue esta llamada?" after calls from numbers outside the address book.
/// </summary>
public sealed record ScreeningDecision(CallerCard Card, BlockReason? BlockReason, bool AskForFeedback)
{
    public bool Reject => BlockReason is not null;
}
