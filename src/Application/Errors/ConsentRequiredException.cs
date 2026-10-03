namespace Tranqui.Application.Errors;

/// <summary>The action needs an optional consent (e.g. contributing contacts) the user has not given.</summary>
public sealed class ConsentRequiredException() : Exception("The user has not given the consent this action requires.");
