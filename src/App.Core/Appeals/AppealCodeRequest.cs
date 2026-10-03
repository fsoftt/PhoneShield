namespace Tranqui.App.Core.Appeals;

/// <summary>An SMS code on its way to <see cref="PhoneNumber"/>; <see cref="SessionInfo"/> lets Firebase check it.</summary>
public sealed record AppealCodeRequest(string PhoneNumber, string SessionInfo);
