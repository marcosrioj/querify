using BaseFaq.AI.Business.Common.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.AI.Business.Common.Infrastructure;

public sealed class FaqDbContextFactory(
    ITenantConnectionStringProvider tenantConnectionStringProvider,
    IConfiguration configuration)
    : IFaqDbContextFactory
{
    public FaqDbContext Create(Guid tenantId)
    {
        var connectionString = tenantConnectionStringProvider.GetConnectionString(tenantId);

        var options = new DbContextOptionsBuilder<FaqDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new FaqDbContext(
            options,
            new IntegrationSessionService(tenantId, ResolveAiUserId(configuration)),
            configuration,
            new StaticTenantConnectionStringProvider(connectionString),
            new HttpContextAccessor());
    }

    private static Guid ResolveAiUserId(IConfiguration configuration)
    {
        return Guid.TryParse(configuration["Ai:UserId"], out var configuredUserId)
            ? configuredUserId
            : Guid.Empty;
    }

    private sealed class IntegrationSessionService(Guid tenantId, Guid userId) : ISessionService
    {
        public Guid GetTenantId(AppEnum app) => tenantId;
        public Guid GetUserId() => userId;
    }

    private sealed class StaticTenantConnectionStringProvider(string connectionString)
        : ITenantConnectionStringProvider
    {
        public string GetConnectionString(Guid tenantId) => connectionString;
    }
}
