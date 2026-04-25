using BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Tenancy;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Dtos.Tenant;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenant;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenant;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenant;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenant;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantList;
using BaseFaq.Tenant.BackOffice.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.Tenant.BackOffice.Test.IntegrationTests.Tests.Tenant;

public class TenantCommandQueryTests
{
    [Fact]
    public async Task CreateTenant_PersistsEntityAndReturnsId()
    {
        using var context = TestContext.Create();

        var userId = Guid.NewGuid();
        await TestDataFactory.SeedUserAsync(context.DbContext, id: userId);
        var handler = new TenantsCreateTenantCommandHandler(context.DbContext, new TestAllowedTenantStore());
        var request = new TenantsCreateTenantCommand
        {
            Slug = "tenant-one",
            Name = "Tenant One",
            Edition = TenantEdition.Free,
            Module = ModuleEnum.QnA,
            ConnectionString = IntegrationTestConnectionStrings.QnA,
            IsActive = true,
            UserId = userId
        };

        var id = await handler.Handle(request, CancellationToken.None);

        var tenant = await context.DbContext.Tenants
            .Include(item => item.TenantUsers)
            .FirstOrDefaultAsync(item => item.Id == id);
        Assert.NotNull(tenant);
        Assert.Equal("tenant-one", tenant!.Slug);
        Assert.Equal("Tenant One", tenant.Name);
        Assert.Equal(TenantEdition.Free, tenant.Edition);
        Assert.Equal(ModuleEnum.QnA, tenant.Module);
        Assert.Equal(request.ConnectionString, tenant.ConnectionString);
        Assert.True(tenant.IsActive);
        Assert.Contains(
            tenant.TenantUsers,
            tenantUser => tenantUser.UserId == userId && tenantUser.Role == TenantUserRoleType.Owner);
    }

    [Fact]
    public async Task UpdateTenant_UpdatesExistingTenant()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext, slug: "old", name: "Old");

        var updatedUserId = Guid.NewGuid();
        await TestDataFactory.SeedUserAsync(context.DbContext, id: updatedUserId);
        var handler = new TenantsUpdateTenantCommandHandler(context.DbContext, new TestAllowedTenantStore());
        var request = new TenantsUpdateTenantCommand
        {
            Id = tenant.Id,
            Slug = "new",
            Name = "New",
            Edition = TenantEdition.Enterprise,
            ConnectionString = IntegrationTestConnectionStrings.CreateNamed("updated"),
            IsActive = false,
            UserId = updatedUserId
        };

        await handler.Handle(request, CancellationToken.None);

        var updated = await context.DbContext.Tenants
            .Include(item => item.TenantUsers)
            .FirstOrDefaultAsync(item => item.Id == tenant.Id);
        Assert.NotNull(updated);
        Assert.Equal("new", updated!.Slug);
        Assert.Equal("New", updated.Name);
        Assert.Equal(TenantEdition.Enterprise, updated.Edition);
        Assert.Equal(request.ConnectionString, updated.ConnectionString);
        Assert.False(updated.IsActive);
        Assert.Contains(
            updated.TenantUsers,
            tenantUser => tenantUser.UserId == updatedUserId && tenantUser.Role == TenantUserRoleType.Owner);
    }

    [Fact]
    public async Task UpdateTenant_ThrowsWhenMissing()
    {
        using var context = TestContext.Create();
        var userId = Guid.NewGuid();
        await TestDataFactory.SeedUserAsync(context.DbContext, id: userId);
        var handler = new TenantsUpdateTenantCommandHandler(context.DbContext, new TestAllowedTenantStore());
        var request = new TenantsUpdateTenantCommand
        {
            Id = Guid.NewGuid(),
            Slug = "missing",
            Name = "Missing",
            Edition = TenantEdition.Free,
            ConnectionString = IntegrationTestConnectionStrings.CreateNamed("missing"),
            IsActive = true,
            UserId = userId
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task DeleteTenant_SoftDeletesEntity()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext, slug: "delete");

        var handler = new TenantsDeleteTenantCommandHandler(context.DbContext, new TestAllowedTenantStore());
        await handler.Handle(new TenantsDeleteTenantCommand { Id = tenant.Id }, CancellationToken.None);

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deleted = await context.DbContext.Tenants.FindAsync(tenant.Id);
        Assert.NotNull(deleted);
        Assert.True(deleted!.IsDeleted);
    }

    [Fact]
    public async Task GetTenant_ReturnsDto()
    {
        using var context = TestContext.Create();
        var tenant = await TestDataFactory.SeedTenantAsync(context.DbContext, slug: "get", name: "Get");
        var ownerUserId = tenant.TenantUsers.Single(tenantUser => tenantUser.Role == TenantUserRoleType.Owner).UserId;

        var handler = new TenantsGetTenantQueryHandler(context.DbContext);
        var result = await handler.Handle(new TenantsGetTenantQuery { Id = tenant.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(tenant.Id, result!.Id);
        Assert.Equal("get", result.Slug);
        Assert.Equal("Get", result.Name);
        Assert.Equal(tenant.Edition, result.Edition);
        Assert.Equal(tenant.Module, result.Module);
        Assert.Equal(string.Empty, result.ConnectionString);
        Assert.Equal(tenant.IsActive, result.IsActive);
        Assert.Equal(ownerUserId, result.UserId);
    }

    [Fact]
    public async Task GetTenant_ReturnsNullWhenMissing()
    {
        using var context = TestContext.Create();
        var handler = new TenantsGetTenantQueryHandler(context.DbContext);

        var result = await handler.Handle(new TenantsGetTenantQuery { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTenantList_ReturnsPagedItems()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedTenantAsync(context.DbContext, name: "Alpha");
        await TestDataFactory.SeedTenantAsync(context.DbContext, name: "Bravo");

        var handler = new TenantsGetTenantListQueryHandler(context.DbContext);
        var request = new TenantsGetTenantListQuery
        {
            Request = new TenantGetAllRequestDto { SkipCount = 0, MaxResultCount = 10 }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetTenantList_SortsByExplicitField()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedTenantAsync(context.DbContext, slug: "alpha", name: "Alpha");
        await TestDataFactory.SeedTenantAsync(context.DbContext, slug: "bravo", name: "Bravo");

        var handler = new TenantsGetTenantListQueryHandler(context.DbContext);
        var request = new TenantsGetTenantListQuery
        {
            Request = new TenantGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "slug DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal("bravo", result.Items[0].Slug);
        Assert.Equal("alpha", result.Items[1].Slug);
    }

    [Fact]
    public async Task GetTenantList_FallsBackToUpdatedDateWhenSortingInvalid()
    {
        using var context = TestContext.Create();
        var first = await TestDataFactory.SeedTenantAsync(context.DbContext, name: "Zulu");
        await TestDataFactory.SeedTenantAsync(context.DbContext, name: "Alpha");
        first.IsActive = !first.IsActive;
        await context.DbContext.SaveChangesAsync();

        var handler = new TenantsGetTenantListQueryHandler(context.DbContext);
        var request = new TenantsGetTenantListQuery
        {
            Request = new TenantGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "unknown DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal("Zulu", result.Items[0].Name);
        Assert.Equal("Alpha", result.Items[1].Name);
    }

    [Fact]
    public async Task GetTenantList_SortsByMultipleFields()
    {
        using var context = TestContext.Create();

        var tenantA = await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: Guid.Parse("00000000-0000-0000-0000-000000000010"),
            slug: "b-slug",
            name: "Same",
            connectionString: IntegrationTestConnectionStrings.CreateNamed("a"));
        var tenantB = await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: Guid.Parse("00000000-0000-0000-0000-000000000011"),
            slug: "a-slug",
            name: "Same",
            connectionString: IntegrationTestConnectionStrings.CreateNamed("b"));

        var handler = new TenantsGetTenantListQueryHandler(context.DbContext);
        var request = new TenantsGetTenantListQuery
        {
            Request = new TenantGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "name ASC, slug ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(tenantB.Id, result.Items[0].Id);
        Assert.Equal(tenantA.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task CreateTenant_ThrowsWhenSlugDuplicated()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedTenantAsync(context.DbContext, slug: "dup");

        var userId = Guid.NewGuid();
        await TestDataFactory.SeedUserAsync(context.DbContext, id: userId);
        var handler = new TenantsCreateTenantCommandHandler(context.DbContext, new TestAllowedTenantStore());
        var request = new TenantsCreateTenantCommand
        {
            Slug = "dup",
            Name = "Duplicate",
            Edition = TenantEdition.Free,
            Module = ModuleEnum.QnA,
            ConnectionString = IntegrationTestConnectionStrings.CreateNamed("dup"),
            IsActive = true,
            UserId = userId
        };

        await Assert.ThrowsAsync<DbUpdateException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetTenantList_AppliesPaginationWindow()
    {
        using var context = TestContext.Create();
        await TestDataFactory.SeedTenantAsync(context.DbContext, name: "Charlie", slug: "charlie");
        await TestDataFactory.SeedTenantAsync(context.DbContext, name: "Alpha", slug: "alpha");
        await TestDataFactory.SeedTenantAsync(context.DbContext, name: "Bravo", slug: "bravo");

        var handler = new TenantsGetTenantListQueryHandler(context.DbContext);
        var request = new TenantsGetTenantListQuery
        {
            Request = new TenantGetAllRequestDto
            {
                SkipCount = 1,
                MaxResultCount = 1,
                Sorting = "name ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Bravo", result.Items[0].Name);
    }
}
