namespace Tranqui.Domain.Reputation;

/// <summary>What the community knows about a number. "In my contacts" is decided on the device, never here.</summary>
public enum CallerStatus
{
    Unknown = 0,
    Identified = 1,
    Spam = 2,
}
