using Microsoft.EntityFrameworkCore;
using Tranqui.Infrastructure.Persistence;

namespace Tranqui.Api.Persistence;

public static class DatabaseMigrationExtensions
{
    /// <summary>Applies pending EF Core migrations. The API runs as a single instance, so startup is a safe place for it.</summary>
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TranquiDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
