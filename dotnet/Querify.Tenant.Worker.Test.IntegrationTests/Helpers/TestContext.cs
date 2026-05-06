using Querify.Common.Architecture.Test.IntegrationTest.Shared.Configuration;
using Querify.Common.Architecture.Test.IntegrationTest.Shared.Database;
using Querify.Common.Architecture.Test.IntegrationTest.Shared.Session;
using Querify.Common.Architecture.Test.IntegrationTest.Shared.Tenancy;
using Querify.Common.EntityFramework.Tenant;
using Microsoft.AspNetCore.Http;

namespace Querify.Tenant.Worker.Test.IntegrationTests.Helpers;

public sealed class TestContext : IDisposable
{
    private readonly SqliteInMemoryDatabase _database;

    private TestContext(
        TenantDbContext dbContext,
        IntegrationTestSessionService sessionService,
        HttpContextAccessor httpContextAccessor,
        SqliteInMemoryDatabase database)
    {
        DbContext = dbContext;
        SessionService = sessionService;
        HttpContextAccessor = httpContextAccessor;
        _database = database;
    }

    public TenantDbContext DbContext { get; }
    public IntegrationTestSessionService SessionService { get; }
    public HttpContextAccessor HttpContextAccessor { get; }

    public static TestContext Create(Guid? tenantId = null, Guid? userId = null, HttpContext? httpContext = null)
    {
        var database = new SqliteInMemoryDatabase();
        var resolvedTenantId = tenantId ?? Guid.NewGuid();
        var resolvedUserId = userId ?? Guid.NewGuid();
        var sessionService = new IntegrationTestSessionService(resolvedTenantId, resolvedUserId);
        var resolvedHttpContext = httpContext ?? IntegrationTestHttpContextFactory.Create("TenantWorkerTest/1.0");
        IntegrationTestHttpContextFactory.ApplyDefaults(resolvedHttpContext, "TenantWorkerTest/1.0");
        var httpContextAccessor = new HttpContextAccessor { HttpContext = resolvedHttpContext };
        var configuration = IntegrationTestConfigurationFactory.Create();
        var tenantConnectionStringProvider =
            new StaticTenantConnectionStringProvider(database.ConnectionString);

        var dbContext = SqliteInMemoryDbContextFactory.Create<TenantDbContext>(
            database,
            options => new TenantDbContext(
                options,
                sessionService,
                configuration,
                tenantConnectionStringProvider,
                httpContextAccessor));

        return new TestContext(
            dbContext,
            sessionService,
            httpContextAccessor,
            database);
    }

    public void Dispose()
    {
        DbContext.Dispose();
        _database.Dispose();
    }
}
