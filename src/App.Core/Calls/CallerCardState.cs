namespace Tranqui.App.Core.Calls;

/// <summary>What the incoming-call popup shows. Each state has its own color, icon and text (never color alone).</summary>
public enum CallerCardState
{
    KnownContact,
    Identified,
    Spam,
    Unknown,
    Offline,
    PrivateNumber,
}
