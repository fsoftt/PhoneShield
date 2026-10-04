using Android.App;
using Android.Telecom;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.History;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Calls;

/// <summary>
/// Android hands every incoming call here before it rings (requires the call screening role). The decision must be
/// answered within ~5 seconds; the community lookup is capped at 2 so there is always time to respond.
/// </summary>
[Service(Exported = true, Permission = "android.permission.BIND_SCREENING_SERVICE")]
[IntentFilter(new[] { "android.telecom.CallScreeningService" })]
public sealed class TranquiCallScreeningService : CallScreeningService
{
    private static readonly CallResponse allow = new CallResponse.Builder().Build()!;

    private CallerOverlay? overlay;

    public override void OnScreenCall(Call.Details callDetails)
    {
        ArgumentNullException.ThrowIfNull(callDetails);

        if (callDetails.CallDirection != CallDirection.Incoming)
        {
            RespondToCall(callDetails, allow);
            return;
        }

        _ = ScreenAsync(callDetails);
    }

    private async Task ScreenAsync(Call.Details callDetails)
    {
        var services = IPlatformApplication.Current?.Services;
        if (services is null)
        {
            RespondToCall(callDetails, allow);
            return;
        }

        ScreeningDecision decision;
        try
        {
            decision = await services.GetRequiredService<CallScreener>()
                .ScreenAsync(callDetails.GetHandle()?.SchemeSpecificPart, CancellationToken.None);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            // Never let a bug make the phone miss a call: when in doubt, let it ring.
            RespondToCall(callDetails, allow);
            return;
        }

        RespondToCall(callDetails, decision.Reject ? Reject() : allow);
        Present(decision, services);
        await services.GetRequiredService<ICallHistory>()
            .AddAsync(CallRecordFactory.From(decision, services.GetRequiredService<TimeProvider>().GetUtcNow()));
    }

    private void Present(ScreeningDecision decision, IServiceProvider services)
    {
        var notifications = new CallNotifications(this);

        if (decision.Reject)
        {
            notifications.ShowBlocked(decision);
        }
        else
        {
            overlay ??= new CallerOverlay(this);
            if (overlay.CanShow)
            {
                overlay.Show(decision.Card, () => _ = services.GetRequiredService<BlockingService>().BlockAsync(decision.Card.Number!));
            }
            else
            {
                notifications.ShowCallerCard(decision.Card);
            }
        }

        if (decision.AskForFeedback && decision.Card.Number is not null)
        {
            notifications.AskForFeedback(decision.Card.Number);
        }

        if (decision.Card is { State: CallerCardState.Offline, Number: { } number })
        {
            _ = IdentifyLaterAsync(number, services, notifications);
        }
    }

    /// <summary>The lookup did not answer in time: keep trying after the call and say who it was (spec §3.2).</summary>
    private static async Task IdentifyLaterAsync(
        PhoneNumber number, IServiceProvider services, CallNotifications notifications)
    {
        try
        {
            if (await services.GetRequiredService<LateIdentification>().RetryAsync(number, CancellationToken.None) is { } card)
            {
                notifications.ShowLateIdentification(card);
            }
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            // Best effort: the call already happened.
        }
    }

    private static CallResponse Reject() =>
        new CallResponse.Builder()
            .SetDisallowCall(true)!
            .SetRejectCall(true)!
            .SetSkipNotification(true)!
            .Build()!;
}
