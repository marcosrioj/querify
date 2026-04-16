using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Public.Test.IntegrationTests.Helpers.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;

public sealed class TestContext : IDisposable
{
    private readonly bool _ownsDatabase;
    private readonly string? _databaseName;
    private readonly string? _adminConnectionString;

    private TestContext(
        QnADbContext dbContext,
        HttpContextAccessor httpContextAccessor,
        Guid tenantId,
        Guid userId,
        string clientKey,
        bool ownsDatabase,
        string? databaseName,
        string? adminConnectionString)
    {
        DbContext = dbContext;
        HttpContextAccessor = httpContextAccessor;
        TenantId = tenantId;
        UserId = userId;
        ClientKey = clientKey;
        _ownsDatabase = ownsDatabase;
        _databaseName = databaseName;
        _adminConnectionString = adminConnectionString;
    }

    public QnADbContext DbContext { get; }
    public HttpContextAccessor HttpContextAccessor { get; }
    public Guid TenantId { get; }
    public Guid UserId { get; }
    public string ClientKey { get; }

    public static TestContext Create(
        Guid? tenantId = null,
        Guid? userId = null,
        string? clientKey = null,
        HttpContext? httpContext = null)
    {
        var database = TestDatabase.Create();
        return CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId,
            userId,
            clientKey,
            httpContext,
            ownsDatabase: true);
    }

    public static TestContext CreateForDatabase(
        string connectionString,
        string adminConnectionString,
        string databaseName,
        Guid? tenantId = null,
        Guid? userId = null,
        string? clientKey = null,
        HttpContext? httpContext = null,
        bool ownsDatabase = false)
    {
        var resolvedTenantId = tenantId ?? Guid.NewGuid();
        var resolvedUserId = userId ?? Guid.NewGuid();
        var resolvedClientKey = clientKey ?? "test-client-key";
        var sessionService = new TestSessionService(resolvedTenantId, resolvedUserId);
        var resolvedHttpContext = httpContext ?? new DefaultHttpContext();
        resolvedHttpContext.Items[ClientKeyContextKeys.ClientKeyItemKey] = resolvedClientKey;
        var httpContextAccessor = new HttpContextAccessor { HttpContext = resolvedHttpContext };

        var options = new DbContextOptionsBuilder<QnADbContext>()
            .UseNpgsql(connectionString)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var tenantConnectionStringProvider = new TestTenantConnectionStringProvider(connectionString);

        var dbContext = new QnADbContext(
            options,
            sessionService,
            configuration,
            tenantConnectionStringProvider,
            httpContextAccessor);

        EnsureDatabase(dbContext);

        return new TestContext(
            dbContext,
            httpContextAccessor,
            resolvedTenantId,
            resolvedUserId,
            resolvedClientKey,
            ownsDatabase,
            databaseName,
            adminConnectionString);
    }

    public void Dispose()
    {
        DbContext.Dispose();

        if (_ownsDatabase && _databaseName is not null && _adminConnectionString is not null)
        {
            TestDatabase.DropDatabase(_adminConnectionString, _databaseName);
        }
    }

    private static void EnsureDatabase(QnADbContext dbContext)
    {
        if (dbContext.Database.GetMigrations().Any())
        {
            dbContext.Database.Migrate();
            return;
        }

        dbContext.Database.EnsureCreated();
    }
}
