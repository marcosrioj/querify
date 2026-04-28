using System.Net;
using BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Configuration;
using BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Database;
using BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Session;
using BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Tenancy;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using Microsoft.AspNetCore.Http;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;

public sealed class TestContext : IDisposable
{
    private readonly SqliteInMemoryDatabase _database;

    private TestContext(
        QnADbContext dbContext,
        IntegrationTestSessionService sessionService,
        HttpContextAccessor httpContextAccessor,
        SqliteInMemoryDatabase database)
    {
        DbContext = dbContext;
        SessionService = sessionService;
        HttpContextAccessor = httpContextAccessor;
        _database = database;
    }

    public QnADbContext DbContext { get; }
    public IntegrationTestSessionService SessionService { get; }
    public HttpContextAccessor HttpContextAccessor { get; }

    public void Dispose()
    {
        DbContext.Dispose();
        _database.Dispose();
    }

    public static TestContext Create(Guid? tenantId = null, Guid? userId = null, HttpContext? httpContext = null)
    {
        var database = new SqliteInMemoryDatabase();
        var resolvedTenantId = tenantId ?? Guid.NewGuid();
        var resolvedUserId = userId ?? Guid.NewGuid();
        var sessionService = new IntegrationTestSessionService(resolvedTenantId, resolvedUserId);
        var resolvedHttpContext = httpContext ?? IntegrationTestHttpContextFactory.Create("QnAPortalTest/1.0");
        IntegrationTestHttpContextFactory.ApplyDefaults(resolvedHttpContext, "QnAPortalTest/1.0");
        var httpContextAccessor = new HttpContextAccessor { HttpContext = resolvedHttpContext };
        var configuration = IntegrationTestConfigurationFactory.Create();
        var tenantConnectionStringProvider =
            new StaticTenantConnectionStringProvider(database.ConnectionString);

        var dbContext = SqliteInMemoryDbContextFactory.Create<QnADbContext>(
            database,
            options => new QnADbContext(
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
}
