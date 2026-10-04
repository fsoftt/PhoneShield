using System.Net;
using System.Text.RegularExpressions;

namespace Tranqui.BackOffice.Tests;

/// <summary>Form posts the way a browser makes them: with the antiforgery token from the page.</summary>
internal static partial class Browser
{
    public static async Task<string> GetPageAsync(this HttpClient client, string path)
    {
        var response = await client.GetAsync(new Uri(path, UriKind.Relative), TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return await response.ReadPageAsync();
    }

    /// <summary>The page as a person reads it (the HTML encoder escapes letters such as "ñ").</summary>
    public static async Task<string> ReadPageAsync(this HttpResponseMessage response) =>
        WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

    /// <summary>Where a redirect points, as a local path and query.</summary>
    public static string RedirectTarget(this HttpResponseMessage response) =>
        response.Headers.Location is { IsAbsoluteUri: true } absolute
            ? absolute.Host == "localhost" ? absolute.PathAndQuery : absolute.ToString()
            : response.Headers.Location!.ToString();

    public static async Task<HttpResponseMessage> PostFormAsync(
        this HttpClient client, string pagePath, string action, params (string Name, string Value)[] fields)
    {
        var page = await client.GetPageAsync(pagePath);
        var token = AntiforgeryToken().Match(page).Groups[1].Value;

        return await client.PostAsync(
            new Uri(action, UriKind.Relative),
            new FormUrlEncodedContent([.. fields.Select(field => KeyValuePair.Create(field.Name, field.Value)), KeyValuePair.Create("__RequestVerificationToken", token)]),
            TestContext.Current.CancellationToken);
    }

    public static Task<HttpResponseMessage> SignInAsync(this HttpClient client, string email, string returnUrl = "") =>
        client.PostFormAsync(
            "/login",
            $"/login{(returnUrl.Length > 0 ? "?ReturnUrl=" + Uri.EscapeDataString(returnUrl) : string.Empty)}",
            ("_handler", "login"),
            ("Input.Email", email),
            ("Input.Password", FakeFirebase.Password));

    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryToken();
}
