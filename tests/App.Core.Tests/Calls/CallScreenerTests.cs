using NSubstitute;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Resources;
using Tranqui.Contracts.Lookups;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Tests.Calls;

public sealed class CallScreenerTests
{
    private const string Number = "+573001234567";

    private readonly IBlockList blockList = Substitute.For<IBlockList>();
    private readonly IDeviceContacts contacts = Substitute.For<IDeviceContacts>();
    private readonly ICallerLookup lookup = Substitute.For<ICallerLookup>();
    private readonly IScreeningSettingsStore settingsStore = Substitute.For<IScreeningSettingsStore>();
    private readonly CallScreener screener;

    public CallScreenerTests()
    {
        settingsStore.Load().Returns(ScreeningSettings.Default);
        screener = new CallScreener(blockList, contacts, lookup, settingsStore);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task HiddenNumber_ShowsPrivateCardAndRings(string? raw)
    {
        var decision = await screener.ScreenAsync(raw, CancellationToken.None);

        decision.Card.State.Should().Be(CallerCardState.PrivateNumber);
        decision.Reject.Should().BeFalse();
    }

    [Fact]
    public async Task HiddenNumber_WhenBlockingPrivateNumbers_IsRejected()
    {
        GivenSettings(ScreeningSettings.Default with { BlockPrivateNumbers = true });

        var decision = await screener.ScreenAsync(null, CancellationToken.None);

        decision.BlockReason.Should().Be(BlockReason.PrivateNumber);
    }

    [Fact]
    public async Task BlockedNumber_IsRejectedWithoutLookingItUp()
    {
        blockList.ContainsAsync(Arg.Is<PhoneNumber>(number => number.E164 == Number)).Returns(true);

        var decision = await screener.ScreenAsync("300 123 4567", CancellationToken.None);

        decision.BlockReason.Should().Be(BlockReason.BlockedByUser);
        await lookup.DidNotReceive().LookupAsync(Arg.Any<PhoneNumber>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ContactNumber_IsGreenWithTheLocalName()
    {
        contacts.FindName(Arg.Any<PhoneNumber>()).Returns("Mamá");

        var decision = await screener.ScreenAsync(Number, CancellationToken.None);

        decision.Card.State.Should().Be(CallerCardState.KnownContact);
        decision.Card.Title.Should().Be("Mamá");
        decision.Card.Subtitle.Should().Be(Texts.KnownContactSubtitle);
        decision.AskForFeedback.Should().BeFalse();
    }

    [Fact]
    public async Task ContactNumberFlaggedByTheCommunity_StaysGreenButWarns()
    {
        contacts.FindName(Arg.Any<PhoneNumber>()).Returns("Banco");
        GivenLookup(new LookupResponse(CallerStatusDto.Spam, "Spam", [], 12, 0));

        var decision = await screener.ScreenAsync(Number, CancellationToken.None);

        decision.Card.State.Should().Be(CallerCardState.KnownContact);
        decision.Card.CommunityFlagsAsSpam.Should().BeTrue();
        decision.Card.Subtitle.Should().Be(Texts.KnownContactButSpamSubtitle);
        decision.Reject.Should().BeFalse();
    }

    [Fact]
    public async Task CommunitySpam_IsRedWithLabelAndReportCount()
    {
        GivenLookup(new LookupResponse(CallerStatusDto.Spam, "Spam Claro", ["Cobranzas"], 23, 0));

        var decision = await screener.ScreenAsync(Number, CancellationToken.None);

        decision.Card.State.Should().Be(CallerCardState.Spam);
        decision.Card.Title.Should().Be("Spam Claro");
        decision.Card.Subtitle.Should().Contain("23");
        decision.Card.Names.Should().Equal("Spam Claro", "Cobranzas");
        decision.Reject.Should().BeFalse();
        decision.AskForFeedback.Should().BeTrue();
    }

    [Fact]
    public async Task CommunitySpam_WhenAutoBlockingSpam_IsRejected()
    {
        GivenSettings(ScreeningSettings.Default with { BlockCommunitySpam = true });
        GivenLookup(new LookupResponse(CallerStatusDto.Spam, null, [], 9, 0));

        var decision = await screener.ScreenAsync(Number, CancellationToken.None);

        decision.BlockReason.Should().Be(BlockReason.CommunitySpam);
        decision.Card.Title.Should().Be(Texts.SpamTitle);
    }

    [Fact]
    public async Task Identified_IsBlueWithSlidableNames()
    {
        GivenLookup(new LookupResponse(CallerStatusDto.Identified, "Pizzería Juan", ["Juan Domicilios"], 0, 8));

        var decision = await screener.ScreenAsync(Number, CancellationToken.None);

        decision.Card.State.Should().Be(CallerCardState.Identified);
        decision.Card.Title.Should().Be("Pizzería Juan");
        decision.Card.Names.Should().Equal("Pizzería Juan", "Juan Domicilios");
    }

    [Fact]
    public async Task NoCommunityData_IsOrangeWithMaskedNumber()
    {
        GivenLookup(new LookupResponse(CallerStatusDto.Unknown, null, [], 0, 0));

        var decision = await screener.ScreenAsync(Number, CancellationToken.None);

        decision.Card.State.Should().Be(CallerCardState.Unknown);
        decision.Card.Title.Should().Be("+57 •••••••567");
    }

    [Fact]
    public async Task LookupUnavailable_IsOfflineAndStillRings()
    {
        var decision = await screener.ScreenAsync(Number, CancellationToken.None);

        decision.Card.State.Should().Be(CallerCardState.Offline);
        decision.Reject.Should().BeFalse();
    }

    [Fact]
    public async Task NumberWithABlockedPrefix_IsRejectedWithoutLookingItUp()
    {
        GivenSettings(ScreeningSettings.Default with { BlockedPrefixes = ["+57300"] });

        var decision = await screener.ScreenAsync(Number, CancellationToken.None);

        decision.BlockReason.Should().Be(BlockReason.Prefix);
        await lookup.DidNotReceive().LookupAsync(Arg.Any<PhoneNumber>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ContactWithABlockedPrefix_StillRings()
    {
        GivenSettings(ScreeningSettings.Default with { BlockedPrefixes = ["+57300"] });
        contacts.FindName(Arg.Any<PhoneNumber>()).Returns("Ana");

        var decision = await screener.ScreenAsync(Number, CancellationToken.None);

        decision.Reject.Should().BeFalse();
    }

    [Fact]
    public async Task InternationalNumber_WhenBlockingInternational_IsRejected()
    {
        GivenSettings(ScreeningSettings.Default with { BlockInternational = true });

        var decision = await screener.ScreenAsync("+1 650 253 0000", CancellationToken.None);

        decision.BlockReason.Should().Be(BlockReason.International);
    }

    [Fact]
    public async Task LocalNumber_WhenBlockingInternational_IsNotRejected()
    {
        GivenSettings(ScreeningSettings.Default with { BlockInternational = true });

        var decision = await screener.ScreenAsync(Number, CancellationToken.None);

        decision.Reject.Should().BeFalse();
    }

    private void GivenSettings(ScreeningSettings settings) => settingsStore.Load().Returns(settings);

    private void GivenLookup(LookupResponse response) =>
        lookup.LookupAsync(Arg.Any<PhoneNumber>(), Arg.Any<CancellationToken>()).Returns(response);
}
