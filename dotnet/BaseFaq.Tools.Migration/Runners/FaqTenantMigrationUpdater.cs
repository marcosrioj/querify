using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.Tools.Migration.Services;
using BaseFaq.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.Tools.Migration.Runners;

internal static class FaqTenantMigrationUpdater
{
    public static void ApplyAll(IConfiguration configuration, string tenantDbConnectionString, AppEnum app)
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
            .Where(item => item.App == ResolveTenantApp(app))
            .Select(item => item.ConnectionString)
            .ToList();

        if (tenantConnectionStrings.Count == 0)
        {
            Console.WriteLine($"No tenants found for {app}.");
            return;
        }

        var uniqueConnections = tenantConnectionStrings
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (uniqueConnections.Count == 0)
        {
            Console.WriteLine($"No valid connection strings found for {app}.");
            return;
        }

        Console.WriteLine($"Applying migrations for {app} ({uniqueConnections.Count} tenant(s))...");

        var index = 1;
        foreach (var connectionString in uniqueConnections)
        {
            Console.WriteLine($"[{index}/{uniqueConnections.Count}] Updating tenant database...");
            ApplyMigration(app, connectionString, sessionService, configuration, tenantConnectionProvider, httpContextAccessor);
            index++;
        }

        Console.WriteLine("Database update completed.");
    }

    private static AppEnum ResolveTenantApp(AppEnum app)
    {
        return app switch
        {
            AppEnum.Faq => AppEnum.Faq,
            AppEnum.QnA => AppEnum.Faq,
            _ => throw new InvalidOperationException($"Database update is not supported for {app}.")
        };
    }

    private static void ApplyMigration(
        AppEnum app,
        string connectionString,
        MigrationsSessionService sessionService,
        IConfiguration configuration,
        NoopTenantConnectionStringProvider tenantConnectionProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        switch (app)
        {
            case AppEnum.Faq:
            {
                var options = new DbContextOptionsBuilder<FaqDbContext>()
                    .UseNpgsql(connectionString)
                    .Options;

                using var faqDbContext = new FaqDbContext(
                    options,
                    sessionService,
                    configuration,
                    tenantConnectionProvider,
                    httpContextAccessor);

                faqDbContext.Database.Migrate();
                break;
            }
            case AppEnum.QnA:
            {
                var options = new DbContextOptionsBuilder<QnADbContext>()
                    .UseNpgsql(connectionString)
                    .Options;

                using var qnaDbContext = new QnADbContext(
                    options,
                    sessionService,
                    configuration,
                    tenantConnectionProvider,
                    httpContextAccessor);

                qnaDbContext.Database.Migrate();
                break;
            }
            default:
                throw new InvalidOperationException($"Database update is not supported for {app}.");
        }
    }
}
