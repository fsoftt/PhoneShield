namespace Tranqui.Application.Abstractions;

/// <summary>The authenticated caller, taken from the validated Firebase token. Never from client-supplied values.</summary>
public interface ICurrentUser
{
    string FirebaseUid { get; }
}
