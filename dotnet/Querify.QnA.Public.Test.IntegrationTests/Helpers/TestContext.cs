using System.Net;
using Querify.Common.Architecture.Test.IntegrationTest.Shared.Configuration;
using Querify.Common.Architecture.Test.IntegrationTest.Shared.Database;
using Querify.Common.Architecture.Test.IntegrationTest.Shared.Session;
using Querify.Common.Architecture.Test.IntegrationTest.Shared.Tenancy;
using Querify.Common.Infrastructure.Core.Constants;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Microsoft.AspNetCore.Http;

namespace Querify.QnA.Public.Test.IntegrationTests.Helpers;

public sealed class TestContext : IDisposable
{
    private readonly SqliteInMemoryDatabase _database;

    private TestContext(
        QnADbContext dbContext,
        HttpContextAccessor httpContextAccessor,
        IntegrationTestSessionService sessionService,
        Guid tenantId,
        Guid userId,
        string clientKey,
        SqliteInMemoryDatabase database)
    {
        DbContext = dbContext;
        HttpContextAccessor = httpContextAccessor;
        SessionService = sessionService;
        TenantId = tenantId;
        UserId = userId;
        ClientKey = clientKey;
        _database = database;
    }

    public QnADbContext DbContext { get; }
    public HttpContextAccessor HttpContextAccessor { get; }
    public IntegrationTestSessionService SessionService { get; }
    public Guid TenantId { get; }
    public Guid UserId { get; }
    public string ClientKey { get; }

    public void Dispose()
    {
        DbContext.Dispose();
        _database.Dispose();
    }

    public static TestContext Create(
        Guid? tenantId = null,
        Guid? userId = null,
        string? clientKey = null,
        HttpContext? httpContext = null)
    {
        var database = new SqliteInMemoryDatabase();
        var resolvedTenantId = tenantId ?? Guid.NewGuid();
        var resolvedUserId = userId ?? Guid.NewGuid();
        var resolvedClientKey = clientKey ?? "test-client-key";
        var sessionService = new IntegrationTestSessionService(resolvedTenantId, resolvedUserId);
        var resolvedHttpContext = httpContext ?? IntegrationTestHttpContextFactory.Create("QnAPublicTest/1.0");
        IntegrationTestHttpContextFactory.ApplyDefaults(resolvedHttpContext, "QnAPublicTest/1.0");
        resolvedHttpContext.Items[ClientKeyContextKeys.ClientKeyItemKey] = resolvedClientKey;
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
            httpContextAccessor,
            sessionService,
            resolvedTenantId,
            resolvedUserId,
            resolvedClientKey,
            database);
    }
}
