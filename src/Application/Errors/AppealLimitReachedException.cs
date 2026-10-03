namespace Tranqui.Application.Errors;

/// <summary>The number already used its verification SMS or its appeal for the current window.</summary>
public sealed class AppealLimitReachedException() : Exception("The appeal limit for this number has been reached.");
