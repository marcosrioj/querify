using Microsoft.EntityFrameworkCore;

namespace Querify.Common.Architecture.Test.IntegrationTest.Shared.Database;

public static class SqliteInMemoryDbContextFactory
{
    public static DbContextOptions<TContext> CreateOptions<TContext>(SqliteInMemoryDatabase database)
        where TContext : DbContext
    {
        return new DbContextOptionsBuilder<TContext>()
            .UseSqlite(database.Connection)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;
    }

    public static TContext Create<TContext>(
        SqliteInMemoryDatabase database,
        Func<DbContextOptions<TContext>, TContext> contextFactory)
        where TContext : DbContext
    {
        var context = contextFactory(CreateOptions<TContext>(database));
        context.Database.EnsureCreated();
        return context;
    }
}
