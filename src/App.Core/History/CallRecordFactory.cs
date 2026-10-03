using Tranqui.App.Core.Calls;

namespace Tranqui.App.Core.History;

public static class CallRecordFactory
{
    public static CallRecord From(ScreeningDecision decision, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(decision);

        return new CallRecord(
            Guid.NewGuid(),
            decision.Card.Number?.E164,
            decision.Card.Number?.E164 ?? decision.Card.Title,
            decision.Card.Title,
            decision.Card.State,
            decision.BlockReason,
            occurredAt);
    }
}
