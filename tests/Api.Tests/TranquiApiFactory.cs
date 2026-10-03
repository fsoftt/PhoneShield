using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Testcontainers.PostgreSql;

namespace Tranqui.Api.Tests;

/// <summary>
/// Hosts the API in memory against a real PostgreSQL container, with throwaway secrets and a local token signing key
/// in place of Google's. Token validation rules (issuer, audience, lifetime, claims) are the production ones.
/// </summary>
public sealed class TranquiApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const int LookupLimitPerHour = 5;

    private readonly PostgreSqlContainer database = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public async ValueTask InitializeAsync() => await database.StartAsync();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Postgres", database.GetConnectionString());
        builder.UseSetting("Firebase:ProjectId", TestTokens.ProjectId);
        builder.UseSetting("PhoneHashing:CurrentKeyVersion", "1");
        builder.UseSetting("PhoneHashing:Keys:1", Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
        builder.UseSetting("ContributorIds:Key", Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
        builder.UseSetting("NameProtection:Key", Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
        builder.UseSetting("RateLimiting:Lookup:PerHour", LookupLimitPerHour.ToString(CultureInfo.InvariantCulture));

        builder.ConfigureServices(services =>
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = null;
                options.Configuration = new OpenIdConnectConfiguration { Issuer = TestTokens.Issuer };
                options.TokenValidationParameters.IssuerSigningKey = TestTokens.SigningKey;
            }));
    }
}
