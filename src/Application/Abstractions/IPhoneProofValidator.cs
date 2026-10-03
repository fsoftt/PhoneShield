namespace Tranqui.Application.Abstractions;

/// <summary>Validates the ID token of a recent Firebase SMS sign-in and returns the number it proves, or null.</summary>
public interface IPhoneProofValidator
{
    Task<string?> VerifiedNumberAsync(string phoneProof, CancellationToken cancellationToken);
}
