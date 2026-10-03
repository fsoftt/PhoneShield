using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Tranqui.Application.Appeals;
using Tranqui.Application.Behaviors;

namespace Tranqui.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(AssemblyReference).Assembly;

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddScoped<AppealGate>();

        return services;
    }
}
