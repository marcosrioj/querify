using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.AI.Persistence.AiDb;

internal sealed class AiDbContextFactory : IDesignTimeDbContextFactory<AiDbContext>
{
    public AiDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("AiDb")
                               ?? configuration.GetConnectionString("DefaultConnection")
                               ?? "Host=localhost;Port=5432;Database=bf_ai_db;Username=postgres;Password=Pass123$;";

        var options = new DbContextOptionsBuilder<AiDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AiDbContext(
            options,
            new DesignTimeSessionService(),
            configuration,
            new DesignTimeTenantConnectionStringProvider(),
            new HttpContextAccessor());
    }

    private sealed class DesignTimeSessionService : ISessionService
    {
        public Guid GetTenantId(AppEnum app) => Guid.Empty;

        public Guid GetUserId() => Guid.Empty;
    }

    private sealed class DesignTimeTenantConnectionStringProvider : ITenantConnectionStringProvider
    {
        public string GetConnectionString(Guid tenantId)
        {
            throw new InvalidOperationException(
                "Tenant connection string provider is not available for design-time AI migrations.");
        }
    }
}