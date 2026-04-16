using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.Tools.Seed.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.Tools.Seed.Infrastructure;

public sealed class DbContextFactory : IDbContextFactory
{
    public TenantDbContext CreateTenantDbContext(
        string connectionString,
        IConfiguration configuration,
        ISessionService sessionService,
        ITenantConnectionStringProvider tenantConnectionStringProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TenantDbContext(
            options,
            sessionService,
            configuration,
            tenantConnectionStringProvider,
            httpContextAccessor);
    }

    public QnADbContext CreateQnADbContext(
        string connectionString,
        IConfiguration configuration,
        ISessionService sessionService,
        ITenantConnectionStringProvider tenantConnectionStringProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        var options = new DbContextOptionsBuilder<QnADbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new QnADbContext(
            options,
            sessionService,
            configuration,
            tenantConnectionStringProvider,
            httpContextAccessor);
    }
}
