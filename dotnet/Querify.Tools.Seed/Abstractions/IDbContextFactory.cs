using Querify.Common.EntityFramework.Tenant;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Querify.Tools.Seed.Abstractions;

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
