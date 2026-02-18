using BaseFaq.AI.Persistence.AiDb;
using BaseFaq.AI.Persistence.AiDb.Extensions;
using BaseFaq.AI.Test.IntegrationTest.Helpers.Generation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BaseFaq.AI.Test.IntegrationTest.Tests.Infrastructure;

public class AiDbForeignKeyIsolationTests
{
    [Fact]
    public void AiDbModel_DoesNotContainForeignKeysToNonAiEntities()
    {
        var services = TestServiceCollectionFactory.Create();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddAiDb("Host=localhost;Port=5432;Database=bf_ai_db;Username=postgres;Password=Pass123$;");

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AiDbContext>();

        const string aiNamespacePrefix = "BaseFaq.AI.Persistence.AiDb.Entities";
        var externalForeignKeys = dbContext.Model
            .GetEntityTypes()
            .SelectMany(entityType => entityType.GetForeignKeys())
            .Where(foreignKey =>
                foreignKey.PrincipalEntityType.ClrType.Namespace is null ||
                !foreignKey.PrincipalEntityType.ClrType.Namespace.StartsWith(
                    aiNamespacePrefix,
                    StringComparison.Ordinal))
            .Select(foreignKey =>
                $"{foreignKey.DeclaringEntityType.ClrType.FullName} -> {foreignKey.PrincipalEntityType.ClrType.FullName}")
            .ToArray();

        Assert.Empty(externalForeignKeys);
    }
}