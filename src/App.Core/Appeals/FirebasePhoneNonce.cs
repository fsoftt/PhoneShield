using System.Security.Cryptography;
using System.Text;
using Tranqui.Domain.Appeals;

namespace Tranqui.App.Core.Appeals;

/// <summary>
/// Firebase accepts a Play Integrity token in place of reCAPTCHA to send an SMS from Android, requested with the
/// SHA-256 of the number as nonce.
/// </summary>
public static class FirebasePhoneNonce
{
    public static string For(string e164) =>
        AppealNonce.ToUrlSafeBase64(SHA256.HashData(Encoding.UTF8.GetBytes(e164)));
}
