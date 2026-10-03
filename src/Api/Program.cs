using System.Globalization;
using System.Text.Json.Serialization;
using Tranqui.Api.Authentication;
using Tranqui.Api.Endpoints;
using Tranqui.Api.Errors;
using Tranqui.Api.Persistence;
using Tranqui.Api.RateLimiting;
using Tranqui.Application;
using Tranqui.Infrastructure;
using Tranqui.Infrastructure.Persistence;
using Serilog;

const string HealthEndpoint = "/health";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // preserveStaticLogger: the static Log stays the bootstrap logger, so several hosts (e.g. tests) can coexist.
    builder.Host.UseSerilog(
        (context, configuration) => configuration.ReadFrom.Configuration(context.Configuration),
        preserveStaticLogger: true);

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddHealthChecks().AddDbContextCheck<TranquiDbContext>();
    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddFirebaseAuthentication(builder.Configuration);
    builder.Services.AddTranquiRateLimiting(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapHealthChecks(HealthEndpoint).AllowAnonymous();
    app.MapRegisterAccount();
    app.MapLookupNumber();
    app.MapReportCall();
    app.MapAcceptContactUpload();
    app.MapUploadContacts();
    app.MapWithdrawContacts();
    app.MapExportMyData();
    app.MapDeleteAccount();
    app.MapRequestAppealVerification();
    app.MapSubmitAppeal();

    await app.MigrateDatabaseAsync();
    await app.RunAsync();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
