using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Tranqui.App.Core.Accounts;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Contacts;
using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Core;

public static class DependencyInjection
{
    /// <summary>
    /// Registers everything except platform services, which the app provides: <see cref="ISecureStore"/>, navigation,
    /// <see cref="IBlockList"/>, <see cref="IDeviceContacts"/>, <see cref="IScreeningSettingsStore"/>,
    /// <see cref="Protection.IProtectionPermissions"/>, <see cref="Dialogs.IDialogService"/>,
    /// <see cref="IDeviceContactSource"/> and <see cref="IContributionState"/>.
    /// </summary>
    public static IServiceCollection AddTranquiCore(this IServiceCollection services, Uri apiBaseAddress, string firebaseApiKey)
    {
        services.AddSingleton(TimeProvider.System);
        services.Configure<FirebaseOptions>(options => options.ApiKey = firebaseApiKey);
        services.AddHttpClient<IFirebaseAuthClient, FirebaseAuthClient>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddTransient<AuthorizationHandler>();

        var refitSettings = new RefitSettings(new SystemTextJsonContentSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() },
        }));
        services.AddRefitClient<ITranquiApi>(refitSettings)
            .ConfigureHttpClient(client => client.BaseAddress = apiBaseAddress)
            .AddHttpMessageHandler<AuthorizationHandler>();

        services.AddTransient<IAccountService, AccountService>();
        services.AddSingleton<ICallerLookup, CachedCallerLookup>();
        services.AddTransient<CallScreener>();
        services.AddTransient<ContactContributionService>();
        services.AddTransient<StartupViewModel>();
        services.AddTransient<SignInViewModel>();
        services.AddTransient<SignUpViewModel>();
        services.AddTransient<VerifyEmailViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<BlockedNumbersViewModel>();
        services.AddTransient<AccountViewModel>();

        return services;
    }
}
