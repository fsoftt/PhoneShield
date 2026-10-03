using System.Reflection;

namespace Tranqui.App;

/// <summary>
/// Build-time settings (see Tranqui.App.csproj): the API address and the Firebase Web API key. Neither is secret;
/// they are injected per build so no environment-specific value is hard-coded.
/// </summary>
internal static class AppSettings
{
    public static Uri ApiBaseAddress { get; } = new(Read("TranquiApiBaseUrl"));

    public static string FirebaseApiKey { get; } = Read("TranquiFirebaseApiKey");

    private static string Read(string key) =>
        typeof(AppSettings).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == key)?.Value
        ?? throw new InvalidOperationException($"Build setting '{key}' is missing.");
}
