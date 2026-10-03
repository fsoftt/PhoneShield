using Microsoft.EntityFrameworkCore;
using Tranqui.Domain.Users;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Infrastructure.Users;

internal sealed class UserRepository(TranquiDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken) =>
        dbContext.Users.FirstOrDefaultAsync(user => user.FirebaseUid == firebaseUid, cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);

    public void Remove(User user) => dbContext.Users.Remove(user);
}
