using Tranqui.App.Core.Resources;
using Tranqui.Contracts.Lookups;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Calls;

/// <summary>
/// Decides what happens with an incoming call (spec §3.1), in priority order: hidden number, the user's block list,
/// the phone's own contacts (green, decided locally), international blocking, then the community lookup.
/// </summary>
public sealed class CallScreener(
    IBlockList blockList,
    IDeviceContacts contacts,
    ICallerLookup lookup,
    IScreeningSettingsStore settingsStore)
{
    /// <summary>Colombia: the launch market. Calls from other country codes count as international.</summary>
    public const int HomeCountryCode = 57;

    public async Task<ScreeningDecision> ScreenAsync(string? rawNumber, CancellationToken cancellationToken)
    {
        var settings = settingsStore.Load();

        if (string.IsNullOrWhiteSpace(rawNumber))
        {
            var card = new CallerCard(CallerCardState.PrivateNumber, Texts.PrivateNumberTitle, Texts.PrivateNumberSubtitle, [], null);
            return new ScreeningDecision(card, settings.BlockPrivateNumbers ? BlockReason.PrivateNumber : null, AskForFeedback: false);
        }

        var number = PhoneNumber.TryParse(rawNumber);
        if (number is null)
        {
            return new ScreeningDecision(UnknownCard(rawNumber, null), null, AskForFeedback: false);
        }

        if (await blockList.ContainsAsync(number))
        {
            return new ScreeningDecision(UnknownCard(number.Masked, number), BlockReason.BlockedByUser, AskForFeedback: false);
        }

        var contactName = contacts.FindName(number);
        if (!string.IsNullOrWhiteSpace(contactName))
        {
            var community = await lookup.LookupAsync(number, cancellationToken);
            var flaggedAsSpam = community?.Status == CallerStatusDto.Spam;
            var subtitle = flaggedAsSpam ? Texts.KnownContactButSpamSubtitle : Texts.KnownContactSubtitle;
            return new ScreeningDecision(
                new CallerCard(CallerCardState.KnownContact, contactName, subtitle, [], number, flaggedAsSpam),
                null,
                AskForFeedback: false);
        }

        if (settings.BlockInternational && number.CountryCode != HomeCountryCode)
        {
            return new ScreeningDecision(UnknownCard(number.Masked, number), BlockReason.International, AskForFeedback: true);
        }

        var response = await lookup.LookupAsync(number, cancellationToken);
        var communityCard = ToCard(response, number);
        var blockAsSpam = settings.BlockCommunitySpam && communityCard.State == CallerCardState.Spam;

        return new ScreeningDecision(communityCard, blockAsSpam ? BlockReason.CommunitySpam : null, AskForFeedback: true);
    }

    private static CallerCard ToCard(LookupResponse? response, PhoneNumber number)
    {
        if (response is null)
        {
            return new CallerCard(CallerCardState.Offline, number.Masked, Texts.OfflineSubtitle, [], number);
        }

        var names = response.DisplayName is null ? [] : new[] { response.DisplayName }.Concat(response.OtherNames).ToList();

        return response.Status switch
        {
            CallerStatusDto.Spam => new CallerCard(
                CallerCardState.Spam,
                response.DisplayName ?? Texts.SpamTitle,
                Texts.Format(Texts.SpamSubtitleFormat, response.SpamReportCount),
                names,
                number,
                CommunityFlagsAsSpam: true),
            CallerStatusDto.Identified => new CallerCard(
                CallerCardState.Identified, response.DisplayName!, Texts.IdentifiedSubtitle, names, number),
            _ => UnknownCard(number.Masked, number),
        };
    }

    private static CallerCard UnknownCard(string title, PhoneNumber? number) =>
        new(CallerCardState.Unknown, title, Texts.UnknownSubtitle, [], number);
}
