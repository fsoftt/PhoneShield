namespace Tranqui.Api.Authentication;

public sealed class FirebaseAuthenticationOptions
{
    public const string SectionName = "Firebase";

    public string ProjectId { get; set; } = string.Empty;
}
