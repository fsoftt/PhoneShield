namespace Tranqui.Application.Errors;

/// <summary>The number, the account or the device already used its SMS or appeal quota.</summary>
public sealed class AppealLimitReachedException() : Exception("The appeal limit has been reached.");
