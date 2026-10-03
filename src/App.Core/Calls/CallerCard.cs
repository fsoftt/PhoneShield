using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Calls;

/// <summary>Content of the incoming-call popup. <see cref="Names"/> holds the alternatives the user can slide through.</summary>
public sealed record CallerCard(
    CallerCardState State,
    string Title,
    string Subtitle,
    IReadOnlyList<string> Names,
    PhoneNumber? Number,
    bool CommunityFlagsAsSpam = false);
