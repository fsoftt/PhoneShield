using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tranqui.Application.Features.LookupNumber;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Features.GetBackOfficeOverview;
using Tranqui.Application.Features.PurgeExpiredData;
using Tranqui.Application.Features.RecalculateReporterReputation;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Domain.Users;
using Tranqui.Infrastructure.Appeals;
using Tranqui.Infrastructure.BackOffice;
using Tranqui.Infrastructure.Integrity;
using Tranqui.Infrastructure.Persistence;
using Tranqui.Infrastructure.PhoneNumbers;
using Tranqui.Infrastructure.Reputation;
using Tranqui.Infrastructure.Users;

namespace Tranqui.Infrastructure;

public static class DependencyInjection
{
    public const string DatabaseConnectionStringName = "Postgres";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        AddPhoneHashing(services);
        AddNameProtection(services);
        AddContributorIds(services);
        AddPlayIntegrity(services);
        AddPersistence(services, configuration);

        return services;
    }

    private static void AddPhoneHashing(IServiceCollection services)
    {
        services.AddOptions<PhoneHashingOptions>()
            .BindConfiguration(PhoneHashingOptions.SectionName)
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<PhoneHashingOptions>, PhoneHashingOptionsValidator>();
        services.AddSingleton<IPhoneNumberHasher, HmacPhoneNumberHasher>();
    }

    private static void AddNameProtection(IServiceCollection services)
    {
        services.AddOptions<NameProtectionOptions>()
            .BindConfiguration(NameProtectionOptions.SectionName)
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<NameProtectionOptions>, NameProtectionOptionsValidator>();
        services.AddSingleton<INameProtector, AesGcmNameProtector>();
    }

    private static void AddContributorIds(IServiceCollection services)
    {
        services.AddOptions<ContributorIdOptions>()
            .BindConfiguration(ContributorIdOptions.SectionName)
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<ContributorIdOptions>, ContributorIdOptionsValidator>();
        services.AddSingleton<IContributorIdProvider, HmacContributorIdProvider>();
        services.AddSingleton<IDeviceKeyProvider, HmacDeviceKeyProvider>();
    }

    private static void AddPlayIntegrity(IServiceCollection services)
    {
        services.AddOptions<PlayIntegrityOptions>().BindConfiguration(PlayIntegrityOptions.SectionName);
        services.AddSingleton<IDeviceIntegrityVerifier, PlayIntegrityVerifier>();
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DatabaseConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{DatabaseConnectionStringName}' is not configured.");

        services.AddDbContext<TranquiDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<TranquiDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReputationSignalsReader, ReputationSignalsReader>();
        services.AddScoped<ISpamReportRepository, SpamReportRepository>();
        services.AddScoped<IContactContributionRepository, ContactContributionRepository>();
        services.AddScoped<IAppealRepository, AppealRepository>();
        services.AddScoped<IExpiredDataPurger, ExpiredDataPurger>();
        services.AddScoped<IBlockSignalRepository, BlockSignalRepository>();
        services.AddScoped<IContributorReputationRepository, ContributorReputationRepository>();
        services.AddScoped<IReporterEvidenceReader, ReporterEvidenceReader>();
        services.AddScoped<IBackOfficeStatistics, BackOfficeStatistics>();
    }
}
