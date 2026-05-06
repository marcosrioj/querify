using Querify.Common.EntityFramework.Tenant;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.Tools.Seed.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Querify.Tools.Seed.Infrastructure;

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
