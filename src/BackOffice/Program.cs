using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Tranqui.BackOffice;
using Tranqui.BackOffice.Api;
using Tranqui.BackOffice.Authentication;
using Tranqui.BackOffice.Components;
using Tranqui.BackOffice.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<BackOfficeOptions>()
    .BindConfiguration(BackOfficeOptions.SectionName)
    .Validate(options => options.ApiBaseUrl is { IsAbsoluteUri: true }, "BackOffice:ApiBaseUrl must be an absolute URL.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.FirebaseApiKey), "BackOffice:FirebaseApiKey is required.")
    .ValidateOnStart();

var keysPath = builder.Configuration.GetSection(BackOfficeOptions.SectionName).Get<BackOfficeOptions>()?.DataProtectionKeysPath;
var dataProtection = builder.Services.AddDataProtection().SetApplicationName("tranqui-backoffice");
if (!string.IsNullOrWhiteSpace(keysPath))
{
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IFirebasePasswordAuth, FirebasePasswordAuth>();
builder.Services.AddTransient<BearerTokenHandler>();
builder.Services.AddHttpClient<AdminApiClient>((services, client) =>
        client.BaseAddress = services.GetRequiredService<IOptions<BackOfficeOptions>>().Value.ApiBaseUrl)
    .AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddScoped<AdminSignIn>();

builder.Services.AddAuthentication(SessionCookie.Scheme)
    .AddCookie(SessionCookie.Scheme, options =>
    {
        options.Cookie.Name = SessionCookie.Name;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
        options.LoginPath = BackOfficeRoutes.Login;
        options.AccessDeniedPath = BackOfficeRoutes.Login;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    CultureInfo[] cultures = [new("es"), new("en")];
    options.DefaultRequestCulture = new("es");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;
});

// Only Caddy can reach the container (deploy/docker-compose.yml publishes no port), and it sets X-Forwarded-*.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHealthChecks();
builder.Services.AddRazorComponents();

var app = builder.Build();

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
}

// Only GETs: re-executing a failed POST would post to the error page.
app.UseWhen(
    context => HttpMethods.IsGet(context.Request.Method),
    branch => branch.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));
app.UseSecurityHeaders();
app.UseRequestLocalization();
app.UseAuthentication();
app.UseMiddleware<SessionTokenMiddleware>();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapHealthChecks("/health");
app.MapFormEndpoints();
app.MapRazorComponents<App>();

await app.RunAsync();

/// <summary>Entry point; public so integration tests can host the app.</summary>
public partial class Program;
