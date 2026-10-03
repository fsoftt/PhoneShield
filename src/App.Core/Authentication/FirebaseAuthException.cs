namespace Tranqui.App.Core.Authentication;

/// <summary>A Firebase Auth error, with a message in Spanish that can be shown to the user as is.</summary>
public sealed class FirebaseAuthException(string code, string userMessage) : Exception(userMessage)
{
    public string Code { get; } = code;
}
