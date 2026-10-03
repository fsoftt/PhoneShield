using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tranqui.Api.Tests;

/// <summary>Hosts the API in memory with throwaway secrets generated per test run.</summary>
public sealed class TranquiApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("PhoneHashing:CurrentKeyVersion", "1");
        builder.UseSetting("PhoneHashing:Keys:1", Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    }
}
