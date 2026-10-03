using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tranqui.Infrastructure.Persistence;

/// <summary>Used only by `dotnet ef` to generate migrations; it never connects to a database.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TranquiDbContext>
{
    private const string PlaceholderConnectionString = "Host=localhost;Database=tranqui";

    public TranquiDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<TranquiDbContext>()
            .UseNpgsql(PlaceholderConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options);
}
