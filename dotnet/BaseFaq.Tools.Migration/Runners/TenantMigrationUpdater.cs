using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Core;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.Tools.Migration.Services;
using BaseFaq.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.Tools.Migration.Runners;

internal static class TenantMigrationUpdater
{
    public static void ApplyAll(IConfiguration configuration, string tenantDbConnectionString, ModuleEnum module)
    {
        var sessionService = new MigrationsSessionService();
        var tenantConnectionProvider = new NoopTenantConnectionStringProvider();
        var httpContextAccessor = new HttpContextAccessor();

        using var tenantDbContext = new TenantDbContext(
            new DbContextOptionsBuilder<TenantDbContext>()
                .UseNpgsql(tenantDbConnectionString)
                .Options,
            sessionService,
            configuration,
            tenantConnectionProvider,
            httpContextAccessor);

        var tenantConnectionStrings = tenantDbContext.Tenants
            .AsNoTracking()
            .Where(item => item.Module == ResolveTenantModule(module))
            .Select(item => item.ConnectionString)
            .ToList();

        if (tenantConnectionStrings.Count == 0)
        {
            Console.WriteLine($"No tenants found for {module}.");
            return;
        }

        var uniqueConnections = tenantConnectionStrings
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (uniqueConnections.Count == 0)
        {
            Console.WriteLine($"No valid connection strings found for {module}.");
            return;
        }

        Console.WriteLine($"Applying migrations for {module} ({uniqueConnections.Count} tenant(s))...");

        var index = 1;
        foreach (var connectionString in uniqueConnections)
        {
            Console.WriteLine($"[{index}/{uniqueConnections.Count}] Updating tenant database...");
            ApplyMigration(module, connectionString, sessionService, configuration, tenantConnectionProvider, httpContextAccessor);
            index++;
        }

        Console.WriteLine("Database update completed.");
    }

    private static ModuleEnum ResolveTenantModule(ModuleEnum module)
    {
        return module == ModuleEnum.QnA
            ? ModuleEnum.QnA
            : throw new InvalidOperationException($"Database update is not supported for {module}.");
    }

    private static void ApplyMigration(
        ModuleEnum module,
        string connectionString,
        MigrationsSessionService sessionService,
        IConfiguration configuration,
        NoopTenantConnectionStringProvider tenantConnectionProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        if (module != ModuleEnum.QnA)
        {
            throw new InvalidOperationException($"Database update is not supported for {module}.");
        }

        var options = new DbContextOptionsBuilder<QnADbContext>()
            .UseNpgsql(connectionString)
            .Options;

        PostgresDatabaseProvisioner.EnsureDatabaseExists(connectionString);
        using var qnaDbContext = new QnADbContext(
            options,
            sessionService,
            configuration,
            tenantConnectionProvider,
            httpContextAccessor);

        qnaDbContext.Database.Migrate();
    }
}
