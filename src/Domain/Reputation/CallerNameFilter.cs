namespace Tranqui.Domain.Reputation;

/// <summary>
/// Whether a name may leave the phone and be shown to others: not a relationship ("Mamá") and not an insult.
/// Applied on the phone before uploading and again on the server.
/// </summary>
public static class CallerNameFilter
{
    public static bool IsShareable(CallerName name) =>
        !PersonalNameFilter.IsPersonal(name) && !OffensiveNameFilter.IsOffensive(name);
}
