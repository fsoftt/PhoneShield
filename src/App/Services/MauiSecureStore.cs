using Tranqui.App.Core.Authentication;

namespace Tranqui.App.Services;

/// <summary>Android Keystore-backed storage for the refresh token.</summary>
internal sealed class MauiSecureStore : ISecureStore
{
    public Task<string?> GetAsync(string key) => SecureStorage.Default.GetAsync(key);

    public Task SetAsync(string key, string value) => SecureStorage.Default.SetAsync(key, value);

    public void Remove(string key) => SecureStorage.Default.Remove(key);
}
