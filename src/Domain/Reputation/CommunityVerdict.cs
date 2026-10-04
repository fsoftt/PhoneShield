namespace Tranqui.Domain.Reputation;

/// <summary>What the community has settled about a number, used to judge how reliable each reporter is.</summary>
public enum CommunityVerdict
{
    Undecided,
    Spam,
    Legitimate,
}
