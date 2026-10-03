using System.Globalization;
using System.Reflection;

namespace Tranqui.App;

/// <summary>
/// Build-time settings (see Tranqui.App.csproj): the API address, the Firebase Web API key and the Google Cloud project
/// number. None is secret; they are injected per build so no environment-specific value is hard-coded.
/// </summary>
internal static class AppSettings
{
    public static Uri ApiBaseAddress { get; } = new(Read("TranquiApiBaseUrl"));

    public static string FirebaseApiKey { get; } = Read("TranquiFirebaseApiKey");

    /// <summary>Google Cloud project number for Play Integrity; 0 lets Google Play use the project linked in Play Console.</summary>
    public static long CloudProjectNumber { get; } = long.Parse(Read("TranquiCloudProjectNumber"), CultureInfo.InvariantCulture);

    private static string Read(string key) =>
        typeof(AppSettings).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == key)?.Value
        ?? throw new InvalidOperationException($"Build setting '{key}' is missing.");
}
