namespace Tranqui.App.Core.Navigation;

/// <summary>Shell routes. Absolute ("//") routes reset the stack so the user cannot go back to a finished flow.</summary>
public static class Routes
{
    public const string Startup = "//startup";
    public const string SignIn = "//sign-in";
    public const string SignUp = "sign-up";
    public const string VerifyEmail = "//verify-email";
    public const string Home = "//main/home";
    public const string Settings = "//main/settings";
    public const string Appeal = "appeal";
    public const string MyReports = "my-reports";
}
