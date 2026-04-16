using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Common.EntityFramework.Tenant.Helpers;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Models.User.Enums;
using BaseFaq.Tools.Seed.Abstractions;
using BaseFaq.Tools.Seed.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tools.Seed.Application;

public sealed class TenantSeedService : ITenantSeedService
{
    private const string SeedTenantSlug = "tenant-001";

    public bool HasEssentialData(TenantDbContext dbContext, TenantSeedRequest request, SeedCounts counts)
    {
        var normalizedUserCount = NormalizeSeedUserCount(counts.UserCount);
        var expectedSeedUserExternalIds = Enumerable
            .Range(1, normalizedUserCount)
            .Select(BuildSeedUserExternalId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var seedUsers = dbContext.Users
            .AsNoTracking()
            .Where(user => expectedSeedUserExternalIds.Contains(user.ExternalId))
            .ToDictionary(user => user.ExternalId, StringComparer.OrdinalIgnoreCase);

        if (seedUsers.Count != expectedSeedUserExternalIds.Count)
        {
            return false;
        }

        var seedTenant = dbContext.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(tenant => tenant.TenantUsers)
            .FirstOrDefault(tenant => tenant.Slug == SeedTenantSlug);

        if (seedTenant is null ||
            seedTenant.IsDeleted ||
            !seedTenant.IsActive ||
            seedTenant.App != AppEnum.Faq ||
            !string.Equals(seedTenant.ConnectionString, request.FaqConnectionString, StringComparison.Ordinal))
        {
            return false;
        }

        var ownerUser = seedUsers[BuildSeedUserExternalId(1)];
        var hasOwnerMembership = seedTenant.TenantUsers.Any(tenantUser =>
            !tenantUser.IsDeleted &&
            tenantUser.UserId == ownerUser.Id &&
            tenantUser.Role == TenantUserRoleType.Owner);

        if (!hasOwnerMembership)
        {
            return false;
        }

        if (normalizedUserCount > 1)
        {
            var memberUser = seedUsers[BuildSeedUserExternalId(2)];
            var hasMemberMembership = seedTenant.TenantUsers.Any(tenantUser =>
                !tenantUser.IsDeleted &&
                tenantUser.UserId == memberUser.Id &&
                tenantUser.Role == TenantUserRoleType.Member);

            if (!hasMemberMembership)
            {
                return false;
            }
        }

        return dbContext.TenantConnections
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Any(connection =>
                !connection.IsDeleted &&
                connection.App == AppEnum.Faq &&
                connection.IsCurrent &&
                connection.ConnectionString == request.FaqConnectionString);
    }

    public EssentialSeedResult EnsureEssentialData(TenantDbContext dbContext, TenantSeedRequest request, SeedCounts counts)
    {
        var seedUsers = EnsureSeedUsers(dbContext, counts.UserCount);
        var seedTenant = EnsureSeedTenant(dbContext, request, seedUsers);
        EnsureCurrentFaqConnection(dbContext, request);

        dbContext.SaveChanges();

        return new EssentialSeedResult(seedTenant.Id);
    }

    private static List<User> EnsureSeedUsers(TenantDbContext dbContext, int count)
    {
        var normalizedCount = NormalizeSeedUserCount(count);
        var expectedExternalIds = Enumerable
            .Range(1, normalizedCount)
            .Select(BuildSeedUserExternalId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingUsers = dbContext.Users
            .IgnoreQueryFilters()
            .Where(user => expectedExternalIds.Contains(user.ExternalId))
            .ToDictionary(user => user.ExternalId, StringComparer.OrdinalIgnoreCase);

        var users = new List<User>();

        for (var index = 1; index <= normalizedCount; index++)
        {
            var externalId = BuildSeedUserExternalId(index);
            if (!existingUsers.TryGetValue(externalId, out var user))
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    GivenName = $"User {index:000}",
                    Email = $"user{index:000}@seed.basefaq.local",
                    ExternalId = externalId
                };

                dbContext.Users.Add(user);
            }

            RestoreEntity(user);
            ApplySeedUserValues(user, index);
            users.Add(user);
        }

        return users;
    }

    private static Tenant EnsureSeedTenant(
        TenantDbContext dbContext,
        TenantSeedRequest request,
        IReadOnlyList<User> users)
    {
        var ownerUser = users.FirstOrDefault();
        var memberUser = users.Skip(1).FirstOrDefault();

        var tenant = dbContext.Tenants
            .IgnoreQueryFilters()
            .Include(entity => entity.TenantUsers)
            .FirstOrDefault(entity => entity.Slug == SeedTenantSlug);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Slug = SeedTenantSlug,
                Name = Tenant.DefaultTenantName,
                Edition = TenantEdition.Free,
                App = AppEnum.Faq,
                ConnectionString = request.FaqConnectionString
            };

            dbContext.Tenants.Add(tenant);
        }

        RestoreEntity(tenant);
        tenant.Slug = SeedTenantSlug;
        tenant.Name = Tenant.DefaultTenantName;
        tenant.Edition = TenantEdition.Free;
        tenant.App = AppEnum.Faq;
        tenant.ConnectionString = request.FaqConnectionString;
        tenant.IsActive = true;

        if (ownerUser is not null)
        {
            TenantUserHelper.SetOwner(tenant.TenantUsers, tenant.Id, ownerUser.Id);
        }

        if (memberUser is not null && memberUser.Id != ownerUser?.Id)
        {
            var existingMember = tenant.TenantUsers
                .FirstOrDefault(tenantUser => tenantUser.UserId == memberUser.Id);

            if (existingMember is null)
            {
                tenant.TenantUsers.Add(new TenantUser
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    UserId = memberUser.Id,
                    Role = TenantUserRoleType.Member
                });
            }
            else
            {
                RestoreEntity(existingMember);
                existingMember.Role = TenantUserRoleType.Member;
            }
        }

        return tenant;
    }

    private static void EnsureCurrentFaqConnection(TenantDbContext dbContext, TenantSeedRequest request)
    {
        var connection = dbContext.TenantConnections
            .IgnoreQueryFilters()
            .ToList()
            .FirstOrDefault(item =>
                item.App == AppEnum.Faq &&
                (item.IsCurrent || item.ConnectionString == request.FaqConnectionString));

        if (connection is null)
        {
            connection = new TenantConnection
            {
                Id = Guid.NewGuid(),
                App = AppEnum.Faq,
                ConnectionString = request.FaqConnectionString,
                IsCurrent = true
            };

            dbContext.TenantConnections.Add(connection);
            return;
        }

        RestoreEntity(connection);
        connection.ConnectionString = request.FaqConnectionString;
        connection.App = AppEnum.Faq;
        connection.IsCurrent = true;
    }

    private static string BuildSeedUserExternalId(int index)
    {
        return $"seed-user-{index:000}";
    }

    private static int NormalizeSeedUserCount(int count)
    {
        return Math.Max(count, 1);
    }

    private static void ApplySeedUserValues(User user, int index)
    {
        user.GivenName = $"User {index:000}";
        user.SurName = index % 3 == 0 ? null : $"Seed {index:000}";
        user.Email = $"user{index:000}@seed.basefaq.local";
        user.ExternalId = BuildSeedUserExternalId(index);
        user.PhoneNumber = index % 4 == 0 ? string.Empty : $"+1-555-01{index:000}";
        user.Role = index % 7 == 0 ? UserRoleType.Admin : UserRoleType.Member;
    }

    private static void RestoreEntity(BaseEntity entity)
    {
        entity.IsDeleted = false;
        entity.DeletedBy = null;
        entity.DeletedDate = null;
    }
}
