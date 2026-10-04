using Tranqui.App.Core.Resources;
using Tranqui.Contracts.Lookups;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Calls;

/// <summary>Builds the caller card for a community lookup result.</summary>
public static class CallerCards
{
    public static CallerCard FromLookup(LookupResponse? response, PhoneNumber number)
    {
        ArgumentNullException.ThrowIfNull(number);

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
            _ => Unknown(number.Masked, number),
        };
    }

    public static CallerCard Unknown(string title, PhoneNumber? number) =>
        new(CallerCardState.Unknown, title, Texts.UnknownSubtitle, [], number);
}
