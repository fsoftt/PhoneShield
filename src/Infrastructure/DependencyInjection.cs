using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Users;
using Tranqui.Infrastructure.Persistence;
using Tranqui.Infrastructure.PhoneNumbers;
using Tranqui.Infrastructure.Users;

namespace Tranqui.Infrastructure;

public static class DependencyInjection
{
    public const string DatabaseConnectionStringName = "Postgres";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        AddPhoneHashing(services);
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

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DatabaseConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{DatabaseConnectionStringName}' is not configured.");

        services.AddDbContext<TranquiDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<TranquiDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
    }
}
