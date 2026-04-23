using BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Tenancy;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.User.Enums;
using BaseFaq.Tenant.BackOffice.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.Tenant.BackOffice.Test.IntegrationTests.Tests.BusinessRules;

public class TenantDbContextBusinessRulesTests
{
    [Fact]
    public async Task GetCurrentTenantConnection_ReturnsCurrentConnection()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedTenantConnectionAsync(context.DbContext, app: AppEnum.QnA, isCurrent: false);
        var current = await TestDataFactory.SeedTenantConnectionAsync(
            context.DbContext,
            app: AppEnum.QnA,
            isCurrent: true);

        var result = await context.DbContext.GetCurrentTenantConnection(AppEnum.QnA);

        Assert.Equal(current.Id, result.Id);
        Assert.True(result.IsCurrent);
    }

    [Fact]
    public async Task GetCurrentTenantConnection_ThrowsWhenMissing()
    {
        using var context = TestContext.Create();

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() =>
                context.DbContext.GetCurrentTenantConnection(AppEnum.QnA));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task GetTenantConnectionString_ReturnsWhenActive()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            connectionString: IntegrationTestConnectionStrings.QnA,
            isActive: true);

        var result = await context.DbContext.GetTenantConnectionString(tenant.Id);

        Assert.Equal(tenant.ConnectionString, result);
    }

    [Fact]
    public async Task GetTenantConnectionString_ThrowsWhenInactive()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            isActive: false);

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => context.DbContext.GetTenantConnectionString(tenant.Id));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task GetTenantConnectionString_ThrowsWhenMissing()
    {
        using var context = TestContext.Create();

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() =>
                context.DbContext.GetTenantConnectionString(Guid.NewGuid()));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task GetTenantConnectionString_ThrowsWhenConnectionStringMissing()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            connectionString: "",
            isActive: true);

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => context.DbContext.GetTenantConnectionString(tenant.Id));

        Assert.Equal(500, exception.ErrorCode);
    }

    [Fact]
    public async Task GetUserId_ReturnsIdForExternalId()
    {
        using var context = TestContext.Create();
        var user = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            externalId: "ext-lookup");

        var result = await context.DbContext.GetUserId("ext-lookup");

        Assert.Equal(user.Id, result);
    }

    [Fact]
    public async Task GetUserId_ThrowsWhenMissing()
    {
        using var context = TestContext.Create();

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => context.DbContext.GetUserId("missing"));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task EnsureUser_ReturnsExistingUserId()
    {
        using var context = TestContext.Create();
        var user = await TestDataFactory.SeedUserAsync(
            context.DbContext,
            givenName: "Pat",
            email: "pat@example.test",
            externalId: "ext-pat",
            role: UserRoleType.Member);

        var result = await context.DbContext.EnsureUser("ext-pat", "Pat", "pat@example.test");

        Assert.Equal(user.Id, result);
    }

    [Fact]
    public async Task EnsureUser_CreatesUserWhenMissing()
    {
        using var context = TestContext.Create();

        var result = await context.DbContext.EnsureUser("ext-new", "New", "new@example.test");

        var user = await context.DbContext.Users.FindAsync(result);
        Assert.NotNull(user);
        Assert.Equal("New", user!.GivenName);
        Assert.Equal("ext-new", user.ExternalId);
        Assert.Equal("new@example.test", user.Email);
        Assert.Equal(UserRoleType.Member, user.Role);
    }

    [Fact]
    public async Task EnsureUser_ThrowsWhenMissingFields()
    {
        using var context = TestContext.Create();

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() =>
                context.DbContext.EnsureUser("", "Name", "mail@example.test"));

        Assert.Equal(404, exception.ErrorCode);
    }
}
