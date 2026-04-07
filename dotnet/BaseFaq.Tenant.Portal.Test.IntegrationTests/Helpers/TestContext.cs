using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Tenant.Portal.Test.IntegrationTests.Helpers.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.Tenant.Portal.Test.IntegrationTests.Helpers;

public sealed class TestContext : IDisposable
{
    private readonly bool _ownsDatabase;
    private readonly string? _databaseName;
    private readonly string? _adminConnectionString;

    private TestContext(
        TenantDbContext dbContext,
        TestSessionService sessionService,
        HttpContextAccessor httpContextAccessor,
        bool ownsDatabase,
        string? databaseName,
        string? adminConnectionString)
    {
        DbContext = dbContext;
        SessionService = sessionService;
        HttpContextAccessor = httpContextAccessor;
        _ownsDatabase = ownsDatabase;
        _databaseName = databaseName;
        _adminConnectionString = adminConnectionString;
    }

    public TenantDbContext DbContext { get; }
    public TestSessionService SessionService { get; }
    public HttpContextAccessor HttpContextAccessor { get; }

    public static TestContext Create(Guid? tenantId = null, Guid? userId = null, HttpContext? httpContext = null)
    {
        var database = TestDatabase.Create();
        return CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId,
            userId,
            httpContext,
            ownsDatabase: true);
    }

    public static TestContext CreateForDatabase(
        string connectionString,
        string adminConnectionString,
        string databaseName,
        Guid? tenantId = null,
        Guid? userId = null,
        HttpContext? httpContext = null,
        bool ownsDatabase = false)
    {
        var resolvedTenantId = tenantId ?? Guid.NewGuid();
        var resolvedUserId = userId ?? Guid.NewGuid();
        var sessionService = new TestSessionService(resolvedTenantId, resolvedUserId);
        var resolvedHttpContext = httpContext ?? CreateHttpContext();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = resolvedHttpContext };

        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseNpgsql(connectionString)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var tenantConnectionStringProvider = new TestTenantConnectionStringProvider(connectionString);

        var dbContext = new TenantDbContext(
            options,
            sessionService,
            configuration,
            tenantConnectionStringProvider,
            httpContextAccessor);

        dbContext.Database.Migrate();

        return new TestContext(
            dbContext,
            sessionService,
            httpContextAccessor,
            ownsDatabase,
            databaseName,
            adminConnectionString);
    }

    private static HttpContext CreateHttpContext()
    {
        return new DefaultHttpContext();
    }

    public void Dispose()
    {
        DbContext.Dispose();

        if (_ownsDatabase && _databaseName is not null && _adminConnectionString is not null)
        {
            TestDatabase.DropDatabase(_adminConnectionString, _databaseName);
        }
    }
}
