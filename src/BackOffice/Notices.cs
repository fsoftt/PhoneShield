namespace Tranqui.BackOffice;

/// <summary>Outcomes passed back to a page after a form post (post/redirect/get).</summary>
public static class Notices
{
    public const string Resolved = "resolved";
    public const string Purged = "purged";
    public const string Forbidden = "forbidden";
    public const string Error = "error";
}
