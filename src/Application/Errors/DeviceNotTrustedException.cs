namespace Tranqui.Application.Errors;

/// <summary>The Play Integrity check failed: not the genuine app, not a genuine device, stale, or for another request.</summary>
public sealed class DeviceNotTrustedException() : Exception("The device integrity check failed.");
