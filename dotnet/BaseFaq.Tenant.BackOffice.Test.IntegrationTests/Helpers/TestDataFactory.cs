using BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Tenancy;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Common.EntityFramework.Tenant.Helpers;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Models.User.Enums;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Test.IntegrationTests.Helpers;

public static class TestDataFactory
{
    public static async Task<Common.EntityFramework.Tenant.Entities.Tenant> SeedTenantAsync(
        TenantDbContext dbContext,
        Guid? id = null,
        string? slug = null,
        string? name = null,
        TenantEdition edition = TenantEdition.Free,
        AppEnum app = AppEnum.QnA,
        string? connectionString = null,
        bool isActive = true,
        Guid? userId = null)
    {
        var ownerUserId = userId ?? Guid.NewGuid();
        await EnsureUserExistsAsync(dbContext, ownerUserId);

        var tenant = new Common.EntityFramework.Tenant.Entities.Tenant
        {
            Id = id ?? Guid.NewGuid(),
            Slug = slug ?? $"tenant-{Guid.NewGuid():N}",
            Name = name ?? "Default Tenant",
            Edition = edition,
            App = app,
            ConnectionString = connectionString ?? IntegrationTestConnectionStrings.QnA,
            IsActive = isActive
        };
        TenantUserHelper.SetOwner(tenant.TenantUsers, tenant.Id, ownerUserId);

        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();
        return tenant;
    }

    public static async Task<TenantConnection> SeedTenantConnectionAsync(
        TenantDbContext dbContext,
        AppEnum app = AppEnum.QnA,
        string? connectionString = null,
        bool isCurrent = true)
    {
        var tenantConnection = new TenantConnection
        {
            App = app,
            ConnectionString = connectionString ?? IntegrationTestConnectionStrings.QnA,
            IsCurrent = isCurrent
        };

        dbContext.TenantConnections.Add(tenantConnection);
        await dbContext.SaveChangesAsync();
        return tenantConnection;
    }

    public static async Task<User> SeedUserAsync(
        TenantDbContext dbContext,
        Guid? id = null,
        string? givenName = null,
        string? surName = null,
        string? email = null,
        string? externalId = null,
        string? phoneNumber = null,
        UserRoleType role = UserRoleType.Member)
    {
        var user = new User
        {
            Id = id ?? Guid.NewGuid(),
            GivenName = givenName ?? "Jordan",
            SurName = surName,
            Email = email ?? $"{Guid.NewGuid():N}@example.test",
            ExternalId = externalId ?? $"ext-{Guid.NewGuid():N}",
            PhoneNumber = phoneNumber ?? "555-0000",
            Role = role
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public static async Task<TenantUser> SeedTenantUserAsync(
        TenantDbContext dbContext,
        Guid tenantId,
        Guid? userId = null,
        TenantUserRoleType role = TenantUserRoleType.Member,
        string? email = null)
    {
        var resolvedUserId = userId ?? Guid.NewGuid();
        await EnsureUserExistsAsync(dbContext, resolvedUserId, email);

        var tenantUser = new TenantUser
        {
            TenantId = tenantId,
            UserId = resolvedUserId,
            Role = role
        };

        dbContext.TenantUsers.Add(tenantUser);
        await dbContext.SaveChangesAsync();
        return tenantUser;
    }

    private static async Task EnsureUserExistsAsync(
        TenantDbContext dbContext,
        Guid userId,
        string? email = null)
    {
        if (await dbContext.Users.AsNoTracking().AnyAsync(user => user.Id == userId))
        {
            return;
        }

        dbContext.Users.Add(new User
        {
            Id = userId,
            GivenName = "Jordan",
            SurName = "Tenant",
            Email = email ?? $"{userId:N}@example.test",
            ExternalId = $"ext-{userId:N}",
            PhoneNumber = "555-0000",
            Role = UserRoleType.Member
        });
        await dbContext.SaveChangesAsync();
    }
}
