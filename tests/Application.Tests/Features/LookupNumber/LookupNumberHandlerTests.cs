using System.Security.Cryptography;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Tranqui.Application.Features.LookupNumber;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Tests.Features.LookupNumber;

public sealed class LookupNumberHandlerTests
{
    private const string RawNumber = "300 123 4567";

    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);
    private static readonly PhoneHash hash = new(RandomNumberGenerator.GetBytes(PhoneHash.SizeInBytes), 1);

    private readonly IPhoneNumberHasher hasher = Substitute.For<IPhoneNumberHasher>();
    private readonly IReputationSignalsReader reader = Substitute.For<IReputationSignalsReader>();
    private readonly INameProtector nameProtector = Substitute.For<INameProtector>();
    private readonly LookupNumberHandler handler;

    public LookupNumberHandlerTests()
    {
        hasher.Hash(Arg.Any<PhoneNumber>()).Returns(hash);
        handler = new LookupNumberHandler(hasher, reader, nameProtector, new FakeTimeProvider(now));
    }

    [Fact]
    public async Task Handle_HashesTheNormalizedNumber()
    {
        reader.ReadAsync(hash, Arg.Any<CancellationToken>()).Returns(ReputationSignals.None);

        await handler.Handle(new LookupNumberQuery(RawNumber), CancellationToken.None);

        hasher.Received(1).Hash(Arg.Is<PhoneNumber>(number => number.E164 == "+573001234567"));
    }

    [Fact]
    public async Task Handle_NoData_ReturnsUnknown()
    {
        reader.ReadAsync(hash, Arg.Any<CancellationToken>()).Returns(ReputationSignals.None);

        var result = await handler.Handle(new LookupNumberQuery(RawNumber), CancellationToken.None);

        result.Status.Should().Be(CallerStatus.Unknown);
        result.DisplayName.Should().BeNull();
        result.OtherNames.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_RankedNames_ReturnsMostUsedAsDisplayNameAndTheRestAsOthers()
    {
        var common = new ProtectedName([1], [1]);
        var rare = new ProtectedName([2], [2]);
        var votes = Votes(common, 4).Concat(Votes(rare, 3)).ToList();
        reader.ReadAsync(hash, Arg.Any<CancellationToken>()).Returns(new ReputationSignals([], votes, 7));
        nameProtector.Unprotect(Arg.Any<PhoneNumber>(), common).Returns("Pizzería Juan");
        nameProtector.Unprotect(Arg.Any<PhoneNumber>(), rare).Returns("Juan Domicilios");

        var result = await handler.Handle(new LookupNumberQuery(RawNumber), CancellationToken.None);

        result.Status.Should().Be(CallerStatus.Identified);
        result.DisplayName.Should().Be("Pizzería Juan");
        result.OtherNames.Should().Equal("Juan Domicilios");
        result.SavedByCount.Should().Be(7);
    }

    private static IEnumerable<NameVote> Votes(ProtectedName name, int count) =>
        Enumerable.Range(0, count).Select(_ =>
            new NameVote(new ContributorId(RandomNumberGenerator.GetBytes(ContributorId.SizeInBytes)), name, now));
}
