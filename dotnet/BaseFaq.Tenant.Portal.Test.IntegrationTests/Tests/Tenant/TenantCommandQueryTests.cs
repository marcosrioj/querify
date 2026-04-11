using BaseFaq.Common.EntityFramework.Tenant.Providers;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.CreateOrUpdateTenants;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.GenerateNewClientKey;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.RefreshAllowedTenantCache;
using BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetAllTenants;
using BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetClientKey;
using BaseFaq.Tenant.Portal.Business.Tenant.Service;
using BaseFaq.Tenant.Portal.Test.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.Tenant.Portal.Test.IntegrationTests.Tests.Tenant;

public class TenantCommandQueryTests
{
    [Fact]
    public async Task GetAllTenants_ReturnsOnlyActiveTenantsForCurrentUser()
    {
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            name: "Current User Tenant",
            app: AppEnum.Faq,
            isActive: true,
            userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            name: "Other User Tenant",
            app: AppEnum.Faq,
            isActive: true,
            userId: Guid.NewGuid());

        var handler = new TenantsGetAllTenantsQueryHandler(context.DbContext, context.SessionService);
        var result = await handler.Handle(new TenantsGetAllTenantsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Current User Tenant", result[0].Name);
        Assert.Equal(AppEnum.Faq, result[0].App);
        Assert.True(result[0].IsActive);
        Assert.Equal(TenantUserRoleType.Owner, result[0].CurrentUserRole);
    }

    [Fact]
    public async Task CreateOrUpdateTenants_CreatesTenantUsingCurrentConnection()
    {
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId);

        await TestDataFactory.SeedUserAsync(context.DbContext, id: currentUserId);
        await TestDataFactory.SeedTenantConnectionAsync(
            context.DbContext,
            app: AppEnum.Faq,
            connectionString: "Host=host.docker.internal;Database=faqdb;Username=tenant;Password=tenant;",
            isCurrent: true);

        var handler = new TenantsCreateOrUpdateTenantsCommandHandler(
            context.DbContext,
            context.SessionService,
            new TestAllowedTenantStore(),
            new TenantPortalAccessService(context.DbContext, context.SessionService));
        var request = new TenantsCreateOrUpdateTenantsCommand
        {
            Name = "Portal Tenant",
            Edition = TenantEdition.Free
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result);
        var tenant = await context.DbContext.Tenants
            .Include(item => item.TenantUsers)
            .FirstOrDefaultAsync(item =>
                item.TenantUsers.Any(tenantUser =>
                    tenantUser.UserId == currentUserId &&
                    tenantUser.Role == TenantUserRoleType.Owner));
        Assert.NotNull(tenant);
        Assert.Equal("Portal Tenant", tenant!.Name);
        Assert.Equal(TenantEdition.Free, tenant.Edition);
        Assert.Equal(AppEnum.Faq, tenant.App);
        Assert.Equal("Host=host.docker.internal;Database=faqdb;Username=tenant;Password=tenant;",
            tenant.ConnectionString);
        Assert.True(tenant.IsActive);
        Assert.Equal("portaltenantfaq", tenant.Slug);
    }

    [Fact]
    public async Task CreateOrUpdateTenants_UpdatesExistingActiveTenant()
    {
        var currentUserId = Guid.NewGuid();
        var selectedTenantId = Guid.NewGuid();
        using var context = TestContext.Create(
            userId: currentUserId,
            httpContext: CreateHttpContextWithTenantId(selectedTenantId));

        await TestDataFactory.SeedTenantConnectionAsync(
            context.DbContext,
            app: AppEnum.Faq,
            connectionString: "Host=host.docker.internal;Database=newdb;Username=tenant;Password=tenant;",
            isCurrent: true);

        var existing = await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: selectedTenantId,
            slug: "old-slug",
            name: "Old Name",
            edition: TenantEdition.Free,
            app: AppEnum.Faq,
            connectionString: "Host=host.docker.internal;Database=olddb;Username=tenant;Password=tenant;",
            isActive: true,
            userId: currentUserId);

        var handler = new TenantsCreateOrUpdateTenantsCommandHandler(
            context.DbContext,
            context.SessionService,
            new TestAllowedTenantStore(),
            new TenantPortalAccessService(context.DbContext, context.SessionService));
        var request = new TenantsCreateOrUpdateTenantsCommand
        {
            TenantId = selectedTenantId,
            Name = "New Name",
            Edition = TenantEdition.Enterprise
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result);
        var updated = await context.DbContext.Tenants.FindAsync(existing.Id);
        Assert.NotNull(updated);
        Assert.Equal(existing.Id, updated!.Id);
        Assert.Equal("newnamefaq", updated.Slug);
        Assert.Equal("New Name", updated.Name);
        Assert.Equal(TenantEdition.Enterprise, updated.Edition);
        Assert.Equal("Host=host.docker.internal;Database=newdb;Username=tenant;Password=tenant;",
            updated.ConnectionString);
        Assert.True(updated.IsActive);
    }

    [Fact]
    public async Task CreateOrUpdateTenants_SkipsWhenNoCurrentConnectionForApp()
    {
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId);

        await TestDataFactory.SeedTenantConnectionAsync(
            context.DbContext,
            app: AppEnum.Faq,
            connectionString: "Host=host.docker.internal;Database=old;Username=tenant;Password=tenant;",
            isCurrent: false);

        var handler = new TenantsCreateOrUpdateTenantsCommandHandler(
            context.DbContext,
            context.SessionService,
            new TestAllowedTenantStore(),
            new TenantPortalAccessService(context.DbContext, context.SessionService));
        var request = new TenantsCreateOrUpdateTenantsCommand
        {
            Name = "Should Skip",
            Edition = TenantEdition.Free
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result);
        var tenantCount = await context.DbContext.TenantUsers.CountAsync(item => item.UserId == currentUserId);
        Assert.Equal(0, tenantCount);
    }

    [Fact]
    public async Task GetAllTenants_ExcludesInactiveTenantsForCurrentUser()
    {
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            name: "Inactive Current User Tenant",
            app: AppEnum.Faq,
            isActive: false,
            userId: currentUserId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            name: "Active Other User Tenant",
            app: AppEnum.Faq,
            isActive: true,
            userId: Guid.NewGuid());

        var handler = new TenantsGetAllTenantsQueryHandler(context.DbContext, context.SessionService);
        var result = await handler.Handle(new TenantsGetAllTenantsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateOrUpdateTenants_CreatesNewTenantWhenInactiveTenantAlreadyExistsForCurrentUser()
    {
        var currentUserId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId);

        await TestDataFactory.SeedTenantConnectionAsync(
            context.DbContext,
            app: AppEnum.Faq,
            connectionString: "Host=host.docker.internal;Database=faqdb;Username=tenant;Password=tenant;",
            isCurrent: true);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            slug: "willconflictfaq",
            name: "Will Conflict",
            app: AppEnum.Faq,
            isActive: false,
            userId: currentUserId);

        var handler = new TenantsCreateOrUpdateTenantsCommandHandler(
            context.DbContext,
            context.SessionService,
            new TestAllowedTenantStore(),
            new TenantPortalAccessService(context.DbContext, context.SessionService));

        var result = await handler.Handle(
            new TenantsCreateOrUpdateTenantsCommand
            {
                Name = "Will Conflict",
                Edition = TenantEdition.Free
            },
            CancellationToken.None);

        Assert.True(result);
        var tenants = await context.DbContext.Tenants
            .Include(item => item.TenantUsers)
            .Where(item => item.TenantUsers.Any(tenantUser => tenantUser.UserId == currentUserId))
            .OrderBy(item => item.IsActive ? 0 : 1)
            .ThenBy(item => item.Name)
            .ToListAsync();
        Assert.Equal(2, tenants.Count);
        Assert.Contains(tenants, item => !item.IsActive && item.Slug == "willconflictfaq");
        Assert.Contains(tenants, item => item.IsActive && item.Slug == "willconflictfaq2");
    }

    [Fact]
    public async Task GetClientKey_ReturnsStoredClientKey()
    {
        var currentUserId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId, tenantId: tenantId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: tenantId,
            app: AppEnum.Faq,
            isActive: true,
            userId: currentUserId,
            clientKey: "my-client-key");

        var handler = new TenantsGetClientKeyQueryHandler(
            new TenantPortalAccessService(context.DbContext, context.SessionService));
        var result = await handler.Handle(new TenantsGetClientKeyQuery { TenantId = tenantId }, CancellationToken.None);

        Assert.Equal("my-client-key", result);
    }

    [Fact]
    public async Task GenerateNewClientKey_UpdatesTenantWithNewValue()
    {
        var currentUserId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId, tenantId: tenantId);

        var tenant = await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: tenantId,
            app: AppEnum.Faq,
            isActive: true,
            userId: currentUserId);

        var handler = new TenantsGenerateNewClientKeyCommandHandler(
            context.DbContext,
            new TenantPortalAccessService(context.DbContext, context.SessionService));
        var generatedKey = await handler.Handle(
            new TenantsGenerateNewClientKeyCommand { TenantId = tenantId },
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(generatedKey));
        Assert.True(generatedKey.Length >= 40);

        var updatedTenant = await context.DbContext.Tenants.FindAsync(tenant.Id);
        Assert.NotNull(updatedTenant);
        Assert.Equal(generatedKey, updatedTenant!.ClientKey);
    }

    [Fact]
    public async Task RefreshAllowedTenantCache_RebuildsCurrentUserAllowedTenants()
    {
        var currentUserId = Guid.NewGuid();
        var selectedTenantId = Guid.NewGuid();
        using var context = TestContext.Create(userId: currentUserId, tenantId: selectedTenantId);

        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            id: selectedTenantId,
            name: "Selected FAQ Tenant",
            app: AppEnum.Faq,
            isActive: true,
            userId: currentUserId);
        var secondFaqTenant = await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            name: "Second FAQ Tenant",
            app: AppEnum.Faq,
            isActive: true,
            userId: currentUserId);
        var tenantAppTenant = await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            name: "Tenant App Workspace",
            app: AppEnum.Tenant,
            isActive: true,
            userId: currentUserId);
        await TestDataFactory.SeedTenantAsync(
            context.DbContext,
            name: "Other User FAQ Tenant",
            app: AppEnum.Faq,
            isActive: true,
            userId: Guid.NewGuid());

        var allowedTenantStore = new TestAllowedTenantStore();
        await allowedTenantStore.SetAllowedTenantIds(
            currentUserId,
            new Dictionary<string, IReadOnlyCollection<Guid>>
            {
                [AppEnum.Faq.ToString()] = Array.Empty<Guid>(),
                [AppEnum.Tenant.ToString()] = Array.Empty<Guid>()
            },
            cancellationToken: CancellationToken.None);

        var handler = new TenantsRefreshAllowedTenantCacheCommandHandler(
            context.SessionService,
            allowedTenantStore,
            new AllowedTenantProvider(context.DbContext));

        var result = await handler.Handle(
            new TenantsRefreshAllowedTenantCacheCommand(),
            CancellationToken.None);

        Assert.True(result);

        var cachedAllowedTenants = await allowedTenantStore.GetAllowedTenantIds(currentUserId, CancellationToken.None);
        Assert.NotNull(cachedAllowedTenants);

        var faqTenantIds = cachedAllowedTenants![AppEnum.Faq.ToString()];
        Assert.Equal(2, faqTenantIds.Count);
        Assert.Contains(selectedTenantId, faqTenantIds);
        Assert.Contains(secondFaqTenant.Id, faqTenantIds);

        var tenantAppTenantIds = cachedAllowedTenants[AppEnum.Tenant.ToString()];
        Assert.Single(tenantAppTenantIds);
        Assert.Contains(tenantAppTenant.Id, tenantAppTenantIds);
    }

    private static HttpContext CreateHttpContextWithTenantId(Guid tenantId)
    {
        return new DefaultHttpContext();
    }
}
