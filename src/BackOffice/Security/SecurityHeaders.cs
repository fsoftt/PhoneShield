namespace Tranqui.BackOffice.Security;

internal static class SecurityHeaders
{
    /// <summary>Only this site's own files; no inline scripts or styles, no framing, forms post only here.</summary>
    private const string ContentSecurityPolicy =
        "default-src 'self'; object-src 'none'; base-uri 'none'; frame-ancestors 'none'; form-action 'self'";

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers.ContentSecurityPolicy = ContentSecurityPolicy;
            headers.XContentTypeOptions = "nosniff";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            headers["X-Robots-Tag"] = "noindex, nofollow";
            await next(context);
        });
}
