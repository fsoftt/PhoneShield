using Microsoft.EntityFrameworkCore;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Users;

namespace Tranqui.Infrastructure.Persistence;

public sealed class TranquiDbContext(DbContextOptions<TranquiDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        await SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TranquiDbContext).Assembly);
}
