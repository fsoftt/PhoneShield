using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Tranqui.App.Core.Calls;
using Tranqui.Contracts.Lookups;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Tests.Calls;

public sealed class LateIdentificationTests
{
    private static readonly PhoneNumber number = PhoneNumber.TryParse("+573001234567")!;

    private readonly ICallerLookup lookup = Substitute.For<ICallerLookup>();
    private readonly FakeTimeProvider time = new(DateTimeOffset.UtcNow);

    [Fact]
    public async Task RetryAsync_AnswerArrivesOnTheSecondTry_ReturnsTheSpamCard()
    {
        lookup.LookupAsync(number, Arg.Any<CancellationToken>())
            .Returns(null, new LookupResponse(CallerStatusDto.Spam, "Spam Claro", [], 12, 0));

        var card = await RunAsync();

        card!.State.Should().Be(CallerCardState.Spam);
        card.Title.Should().Be("Spam Claro");
    }

    [Fact]
    public async Task RetryAsync_UnknownNumber_ReturnsNothingToTell()
    {
        lookup.LookupAsync(number, Arg.Any<CancellationToken>()).Returns(new LookupResponse(CallerStatusDto.Unknown, null, [], 0, 0));

        (await RunAsync()).Should().BeNull();
        await lookup.Received(1).LookupAsync(number, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetryAsync_StillOffline_GivesUpAfterTheLastDelay()
    {
        lookup.LookupAsync(number, Arg.Any<CancellationToken>()).Returns((LookupResponse?)null);

        (await RunAsync()).Should().BeNull();
        await lookup.Received(LateIdentification.RetryDelays.Count).LookupAsync(number, Arg.Any<CancellationToken>());
    }

    private async Task<CallerCard?> RunAsync()
    {
        var retry = new LateIdentification(lookup, time).RetryAsync(number, CancellationToken.None);
        foreach (var delay in LateIdentification.RetryDelays)
        {
            await Task.Yield();
            time.Advance(delay);
        }

        return await retry;
    }
}
