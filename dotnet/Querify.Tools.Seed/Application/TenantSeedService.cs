using Querify.Common.EntityFramework.Core.Entities;
using Querify.Common.EntityFramework.Tenant;
using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Models.Common.Enums;
using Querify.Models.Tenant.Enums;
using Querify.Models.User.Enums;
using Querify.Tools.Seed.Abstractions;
using Querify.Tools.Seed.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tools.Seed.Application;

public sealed class TenantSeedService : ITenantSeedService
{
    private const string SeedTenantSlug = "tenant-001";
    private const string SeedTenantName = "Querify Seed Workspace";
    private const string SeedTenantClientKey = "seed-qna-public";

    private static readonly SeedUserProfile[] SeedUserProfiles =
    [
        new("Ava", "Chen", "ava.chen@seed.querify.local", "+1-555-010-0101", "en-US", "America/Vancouver", UserRoleType.Admin),
        new("Mateo", "Silva", "mateo.silva@seed.querify.local", "+1-555-010-0102", "en-US", "America/Toronto", UserRoleType.Member),
        new("Priya", "Raman", "priya.raman@seed.querify.local", "+1-555-010-0103", "en-US", "America/New_York", UserRoleType.Admin),
        new("Noah", "Brooks", "noah.brooks@seed.querify.local", "+1-555-010-0104", "en-US", "America/Chicago", UserRoleType.Member),
        new("Sofia", "Patel", "sofia.patel@seed.querify.local", "+1-555-010-0105", "en-US", "America/Los_Angeles", UserRoleType.Member),
        new("Lena", "Park", "lena.park@seed.querify.local", "+1-555-010-0106", "en-US", "America/Vancouver", UserRoleType.Member)
    ];

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
            seedTenant.Name != SeedTenantName ||
            seedTenant.ClientKey != SeedTenantClientKey ||
            seedTenant.Module != ModuleEnum.QnA ||
            !string.Equals(seedTenant.ConnectionString, request.QnAConnectionString, StringComparison.Ordinal))
        {
            return false;
        }

        for (var index = 1; index <= normalizedUserCount; index++)
        {
            var seedUser = seedUsers[BuildSeedUserExternalId(index)];
            var expectedRole = index == 1 ? TenantUserRoleType.Owner : TenantUserRoleType.Member;
            var hasMembership = seedTenant.TenantUsers.Any(tenantUser =>
                !tenantUser.IsDeleted &&
                tenantUser.UserId == seedUser.Id &&
                tenantUser.Role == expectedRole);

            if (!hasMembership)
            {
                return false;
            }
        }

        return dbContext.TenantConnections
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Any(connection =>
                !connection.IsDeleted &&
                connection.Module == ModuleEnum.QnA &&
                connection.IsCurrent &&
                connection.ConnectionString == request.QnAConnectionString);
    }

    public EssentialSeedResult EnsureEssentialData(TenantDbContext dbContext, TenantSeedRequest request, SeedCounts counts)
    {
        var seedUsers = EnsureSeedUsers(dbContext, counts.UserCount);
        var seedTenant = EnsureSeedTenant(dbContext, request, seedUsers);
        EnsureCurrentQnAConnection(dbContext, request);

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
                var profile = ResolveUserProfile(index);
                user = new User
                {
                    Id = Guid.NewGuid(),
                    GivenName = profile.GivenName,
                    Email = profile.Email,
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
                Name = SeedTenantName,
                Edition = TenantEdition.Free,
                Module = ModuleEnum.QnA,
                ConnectionString = request.QnAConnectionString,
                ClientKey = SeedTenantClientKey
            };

            dbContext.Tenants.Add(tenant);
        }

        RestoreEntity(tenant);
        tenant.Slug = SeedTenantSlug;
        tenant.Name = SeedTenantName;
        tenant.Edition = TenantEdition.Free;
        tenant.Module = ModuleEnum.QnA;
        tenant.ConnectionString = request.QnAConnectionString;
        tenant.ClientKey = SeedTenantClientKey;
        tenant.IsActive = true;

        for (var index = 0; index < users.Count; index++)
        {
            var role = index == 0 ? TenantUserRoleType.Owner : TenantUserRoleType.Member;
            EnsureTenantMembership(tenant, users[index], role);
        }

        return tenant;
    }

    private static void EnsureCurrentQnAConnection(TenantDbContext dbContext, TenantSeedRequest request)
    {
        var connection = dbContext.TenantConnections
            .IgnoreQueryFilters()
            .ToList()
            .FirstOrDefault(item =>
                item.Module == ModuleEnum.QnA &&
                (item.IsCurrent || item.ConnectionString == request.QnAConnectionString));

        if (connection is null)
        {
            connection = new TenantConnection
            {
                Id = Guid.NewGuid(),
                Module = ModuleEnum.QnA,
                ConnectionString = request.QnAConnectionString,
                IsCurrent = true
            };

            dbContext.TenantConnections.Add(connection);
            return;
        }

        RestoreEntity(connection);
        connection.ConnectionString = request.QnAConnectionString;
        connection.Module = ModuleEnum.QnA;
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
        var profile = ResolveUserProfile(index);
        user.GivenName = profile.GivenName;
        user.SurName = profile.SurName;
        user.Email = profile.Email;
        user.ExternalId = BuildSeedUserExternalId(index);
        user.PhoneNumber = profile.PhoneNumber;
        user.Language = profile.Language;
        user.TimeZone = profile.TimeZone;
        user.Role = profile.Role;
    }

    private static void EnsureTenantMembership(Tenant tenant, User user, TenantUserRoleType role)
    {
        var existingMembership = tenant.TenantUsers
            .FirstOrDefault(tenantUser => tenantUser.UserId == user.Id);

        if (existingMembership is null)
        {
            tenant.TenantUsers.Add(new TenantUser
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                UserId = user.Id,
                Role = role
            });
            return;
        }

        RestoreEntity(existingMembership);
        existingMembership.TenantId = tenant.Id;
        existingMembership.UserId = user.Id;
        existingMembership.Role = role;
    }

    private static SeedUserProfile ResolveUserProfile(int index)
    {
        if (index <= SeedUserProfiles.Length)
        {
            return SeedUserProfiles[index - 1];
        }

        return new SeedUserProfile(
            GivenName: $"Seed User {index:000}",
            SurName: "Operator",
            Email: $"user{index:000}@seed.querify.local",
            PhoneNumber: $"+1-555-010-{index:0000}",
            Language: "en-US",
            TimeZone: "America/Vancouver",
            Role: index % 5 == 0 ? UserRoleType.Admin : UserRoleType.Member);
    }

    private static void RestoreEntity(BaseEntity entity)
    {
        entity.IsDeleted = false;
        entity.DeletedBy = null;
        entity.DeletedDate = null;
    }

    private sealed record SeedUserProfile(
        string GivenName,
        string? SurName,
        string Email,
        string PhoneNumber,
        string Language,
        string TimeZone,
        UserRoleType Role);
}
