using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.Tools.Seed.Abstractions;

public interface IDbContextFactory
{
    TenantDbContext CreateTenantDbContext(
        string connectionString,
        IConfiguration configuration,
        ISessionService sessionService,
        ITenantConnectionStringProvider tenantConnectionStringProvider,
        IHttpContextAccessor httpContextAccessor);

    QnADbContext CreateQnADbContext(
        string connectionString,
        IConfiguration configuration,
        ISessionService sessionService,
        ITenantConnectionStringProvider tenantConnectionStringProvider,
        IHttpContextAccessor httpContextAccessor);
}
