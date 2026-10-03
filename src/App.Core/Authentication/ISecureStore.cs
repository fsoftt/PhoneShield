namespace Tranqui.App.Core.Authentication;

/// <summary>Encrypted device storage (Android Keystore-backed in the app).</summary>
public interface ISecureStore
{
    Task<string?> GetAsync(string key);

    Task SetAsync(string key, string value);

    void Remove(string key);
}
