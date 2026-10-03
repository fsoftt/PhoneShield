namespace Tranqui.Application.Errors;

/// <summary>The account is younger than the minimum age to appeal.</summary>
public sealed class AppealAccountTooNewException() : Exception("The account is too new to appeal.");
