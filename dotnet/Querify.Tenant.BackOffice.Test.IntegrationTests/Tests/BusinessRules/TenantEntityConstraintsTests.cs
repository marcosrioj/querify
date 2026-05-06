using Querify.Common.Architecture.Test.IntegrationTest.Shared.Tenancy;
using Querify.Models.Common.Enums;
using Querify.Models.Tenant.Enums;
using Querify.Tenant.BackOffice.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Querify.Tenant.BackOffice.Test.IntegrationTests.Tests.BusinessRules;

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
            connectionString: IntegrationTestConnectionStrings.CreateNamed("a"));

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            TestDataFactory.SeedTenantAsync(
                context.DbContext,
                slug: "unique-slug",
                name: "Tenant B",
                connectionString: IntegrationTestConnectionStrings.CreateNamed("b")));
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
