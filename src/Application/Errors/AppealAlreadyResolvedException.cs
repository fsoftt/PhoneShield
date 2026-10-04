namespace Tranqui.Application.Errors;

/// <summary>Only pending spam reviews can be resolved.</summary>
public sealed class AppealAlreadyResolvedException() : Exception("The appeal is not pending.");
