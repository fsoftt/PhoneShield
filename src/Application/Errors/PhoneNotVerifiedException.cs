namespace Tranqui.Application.Errors;

/// <summary>The phone proof is missing, invalid or too old.</summary>
public sealed class PhoneNotVerifiedException() : Exception("The phone number was not verified.");
