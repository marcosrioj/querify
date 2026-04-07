using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.BackOffice.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.Tenant.BackOffice.Test.IntegrationTests.Tests.BusinessRules;

public class TenantEntityConstraintsTests
{
    [Fact]
    public async Task Tenants_RequireUniqueSlug()
    {
        using var context = TestContext.Create();

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            slug: "unique-slug",
            name: "Tenant A",
            connectionString: "Host=host.docker.internal;Database=a;Username=tenant;Password=tenant;");

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            TestDataFactory.SeedTenantAsync(
                context.DbContext,
                slug: "unique-slug",
                name: "Tenant B",
                connectionString: "Host=host.docker.internal;Database=b;Username=tenant;Password=tenant;"));
    }

    [Fact]
    public async Task TenantUsers_RequireUniqueTenantAndUserMembership()
    {
        using var context = TestContext.Create();
        var userId = Guid.NewGuid();
        await TestDataFactory.SeedUserAsync(context.DbContext, id: userId);

        var tenant = await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            slug: "tenant-a",
            name: "Tenant A",
            userId: userId);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            TestDataFactory.SeedTenantUserAsync(
                context.DbContext,
                tenant.Id,
                userId: userId,
                role: TenantUserRoleType.Member));
    }
}
