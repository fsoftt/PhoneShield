using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Calls;
using Tranqui.Contracts.Lookups;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Tests.Calls;

public sealed class CachedCallerLookupTests
{
    private static readonly PhoneNumber number = PhoneNumber.TryParse("+573001234567")!;
    private static readonly LookupResponse response = new(CallerStatusDto.Identified, "Pizzería Juan", [], 0, 5);

    private readonly ITranquiApi api = Substitute.For<ITranquiApi>();
    private readonly FakeTimeProvider time = new(new DateTimeOffset(2026, 10, 3, 12, 0, 0, TimeSpan.Zero));
    private readonly CachedCallerLookup lookup;

    public CachedCallerLookupTests()
    {
        api.LookupAsync(Arg.Any<LookupRequest>(), Arg.Any<CancellationToken>()).Returns(response);
        lookup = new CachedCallerLookup(api, time);
    }

    [Fact]
    public async Task Lookup_SendsTheNormalizedNumber()
    {
        (await lookup.LookupAsync(number, CancellationToken.None)).Should().Be(response);

        await api.Received(1).LookupAsync(new LookupRequest("+573001234567"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Lookup_SameNumberWithinCacheLifetime_IsServedFromCache()
    {
        await lookup.LookupAsync(number, CancellationToken.None);
        time.Advance(CachedCallerLookup.CacheLifetime - TimeSpan.FromMinutes(1));

        await lookup.LookupAsync(number, CancellationToken.None);

        await api.Received(1).LookupAsync(Arg.Any<LookupRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Lookup_AfterCacheLifetime_AsksAgain()
    {
        await lookup.LookupAsync(number, CancellationToken.None);
        time.Advance(CachedCallerLookup.CacheLifetime);

        await lookup.LookupAsync(number, CancellationToken.None);

        await api.Received(2).LookupAsync(Arg.Any<LookupRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Lookup_NetworkFailure_ReturnsNull()
    {
        api.LookupAsync(Arg.Any<LookupRequest>(), Arg.Any<CancellationToken>()).ThrowsAsync(new HttpRequestException());

        (await lookup.LookupAsync(number, CancellationToken.None)).Should().BeNull();
    }
}
