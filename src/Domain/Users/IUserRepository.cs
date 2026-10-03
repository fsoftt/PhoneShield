namespace Tranqui.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken);

    void Add(User user);
}
