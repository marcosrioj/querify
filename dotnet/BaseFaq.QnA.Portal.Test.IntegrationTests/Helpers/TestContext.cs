using System.Net;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;

public sealed class TestContext : IDisposable
{
    private readonly string? _adminConnectionString;
    private readonly string? _databaseName;
    private readonly bool _ownsDatabase;

    private TestContext(
        QnADbContext dbContext,
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

    public QnADbContext DbContext { get; }
    public TestSessionService SessionService { get; }
    public HttpContextAccessor HttpContextAccessor { get; }

    public void Dispose()
    {
        DbContext.Dispose();

        if (_ownsDatabase && _databaseName is not null && _adminConnectionString is not null)
            TestDatabase.DropDatabase(_adminConnectionString, _databaseName);
    }

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
            true);
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
        var resolvedHttpContext = httpContext ?? new DefaultHttpContext();
        resolvedHttpContext.Connection.RemoteIpAddress ??= IPAddress.Loopback;
        if (string.IsNullOrWhiteSpace(resolvedHttpContext.Request.Headers.UserAgent))
            resolvedHttpContext.Request.Headers.UserAgent = "QnAPortalTest/1.0";
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
        dbContext.Database.Migrate();

        return new TestContext(
            dbContext,
            sessionService,
            httpContextAccessor,
            ownsDatabase,
            databaseName,
            adminConnectionString);
    }
}