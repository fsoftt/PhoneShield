using Microsoft.EntityFrameworkCore;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Reputation;
using Tranqui.Domain.Users;

namespace Tranqui.Infrastructure.Persistence;

public sealed class TranquiDbContext(DbContextOptions<TranquiDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    public DbSet<SpamReport> SpamReports => Set<SpamReport>();

    public DbSet<ContactContribution> ContactContributions => Set<ContactContribution>();

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        await SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TranquiDbContext).Assembly);
}
