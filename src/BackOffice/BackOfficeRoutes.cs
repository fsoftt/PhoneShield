namespace Tranqui.BackOffice;

public static class BackOfficeRoutes
{
    public const string Home = "/";
    public const string Login = "/login";
    public const string Logout = "/logout";
    public const string Appeals = "/appeals";
    public const string Purge = "/purges";

    public static string Resolution(Guid appealId) => $"/appeals/{appealId}/resolution";
}
