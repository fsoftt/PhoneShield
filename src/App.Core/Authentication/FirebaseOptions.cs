namespace Tranqui.App.Core.Authentication;

/// <summary>The Firebase project's Web API key. Public by design (it ships in every client), not a secret.</summary>
public sealed class FirebaseOptions
{
    public string ApiKey { get; set; } = string.Empty;
}
