using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Infrastructure.PhoneNumbers;

namespace Tranqui.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<PhoneHashingOptions>()
            .BindConfiguration(PhoneHashingOptions.SectionName)
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<PhoneHashingOptions>, PhoneHashingOptionsValidator>();
        services.AddSingleton<IPhoneNumberHasher, HmacPhoneNumberHasher>();

        return services;
    }
}
