#if DEBUG
using Microsoft.Extensions.Logging;
#endif
using Tranqui.App.Calls;
using Tranqui.App.Core;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Contacts;
using Tranqui.App.Core.Dialogs;
using Tranqui.App.Core.Navigation;
using Tranqui.App.Core.Protection;
using Tranqui.App.Services;
using Tranqui.App.Views;

namespace Tranqui.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddTranquiCore(AppSettings.ApiBaseAddress, AppSettings.FirebaseApiKey);
        builder.Services.AddSingleton<ISecureStore, MauiSecureStore>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<IBlockList, PreferencesBlockList>();
        builder.Services.AddSingleton<IScreeningSettingsStore, PreferencesScreeningSettingsStore>();
        builder.Services.AddSingleton<IDeviceContacts, AndroidDeviceContacts>();
        builder.Services.AddSingleton<IProtectionPermissions, AndroidProtectionPermissions>();
        builder.Services.AddSingleton<IDialogService, ShellDialogService>();
        builder.Services.AddSingleton<IDeviceContactSource, AndroidDeviceContactSource>();
        builder.Services.AddSingleton<IContributionState, PreferencesContributionState>();

        builder.Services.AddTransient<StartupPage>();
        builder.Services.AddTransient<SignInPage>();
        builder.Services.AddTransient<SignUpPage>();
        builder.Services.AddTransient<VerifyEmailPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<BlockedNumbersPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<AccountPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
