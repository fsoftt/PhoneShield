using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Calls;

/// <summary>
/// When the lookup did not answer in time during the call, keep trying for a little while afterwards (spec §3.2) and
/// tell the user who it was if the community knows the number.
/// </summary>
public sealed class LateIdentification(ICallerLookup lookup, TimeProvider timeProvider)
{
    public static readonly IReadOnlyList<TimeSpan> RetryDelays =
        [TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(20), TimeSpan.FromMinutes(1)];

    /// <returns>The identified or spam card, or null if the number is unknown or still unreachable.</returns>
    public async Task<CallerCard?> RetryAsync(PhoneNumber number, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(number);

        foreach (var delay in RetryDelays)
        {
            await Task.Delay(delay, timeProvider, cancellationToken);
            if (await lookup.LookupAsync(number, cancellationToken) is { } response)
            {
                var card = CallerCards.FromLookup(response, number);
                return card.State is CallerCardState.Spam or CallerCardState.Identified ? card : null;
            }
        }

        return null;
    }
}
