namespace Tranqui.Domain.Reputation;

/// <summary>Derives the pseudonymous contributor id of an account. The secret key never leaves the implementation.</summary>
public interface IContributorIdProvider
{
    ContributorId FromFirebaseUid(string firebaseUid);
}
