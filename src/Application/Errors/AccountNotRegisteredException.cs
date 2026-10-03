namespace Tranqui.Application.Errors;

/// <summary>The caller has a valid Firebase token but has not registered (and accepted the terms) yet.</summary>
public sealed class AccountNotRegisteredException() : Exception("The account must be registered before contributing.");
