using System.Security.Claims;
using BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Configuration;
using BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Database;
using BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Session;
using BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Tenancy;
using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Providers;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BaseFaq.Tenant.BackOffice.Test.IntegrationTests.Tests.BusinessRules;

public class UserIdProviderTests
{
    [Fact]
    public async Task GetUserId_RebuildsCacheWhenCachedUserNoLongerExists()
    {
        const string externalUserId = "auth0|stale-cache-user";
        const string email = "stale-cache-user@example.test";

        using var database = new SqliteInMemoryDatabase();
        using var serviceProvider = BuildServiceProvider(database, externalUserId, email);
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var userIdProvider = scope.ServiceProvider.GetRequiredService<IUserIdProvider>();
        var firstUserId = userIdProvider.GetUserId();
        var firstUser = await dbContext.Users.FindAsync(firstUserId);

        Assert.NotNull(firstUser);

        await dbContext.Users
            .Where(user => user.Id == firstUserId)
            .ExecuteDeleteAsync();
        dbContext.ChangeTracker.Clear();

        var secondUserId = userIdProvider.GetUserId();
        var secondUser = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(user => user.Id == secondUserId);

        Assert.NotEqual(firstUserId, secondUserId);
        Assert.Equal(externalUserId, secondUser.ExternalId);
        Assert.Equal(email, secondUser.Email);
    }

    private static ServiceProvider BuildServiceProvider(
        SqliteInMemoryDatabase database,
        string externalUserId,
        string email)
    {
        var httpContext = IntegrationTestHttpContextFactory.Create(
            "UserIdProviderTests/1.0",
            context =>
            {
                context.User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        [
                            new Claim("sub", externalUserId),
                            new Claim("name", "Stale Cache User"),
                            new Claim("email", email)
                        ],
                        "Test"));
            });

        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(IntegrationTestConfigurationFactory.Create());
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
        services.AddSingleton<ITenantConnectionStringProvider>(
            new StaticTenantConnectionStringProvider(database.ConnectionString));
        services.AddMemoryCache();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IUserIdProvider, UserIdProvider>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddDbContext<TenantDbContext>(options =>
            options
                .UseSqlite(database.Connection)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());

        return services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
    }
}
