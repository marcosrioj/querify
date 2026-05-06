using System.Net;
using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Attributes;
using Querify.Common.Infrastructure.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Common.EntityFramework.Tenant.Providers;

public sealed class UserIdProvider(IServiceProvider serviceProvider) : IUserIdProvider
{
    private static readonly TimeSpan UserIdCacheDuration = TimeSpan.FromMinutes(30);

    public Guid GetUserId()
    {
        var claimService = serviceProvider.GetRequiredService<IClaimService>();
        var externalUserId = claimService.GetExternalUserId();
        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var endpoint = httpContextAccessor?.HttpContext?.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<SkipTenantAccessValidationAttribute>() is not null)
            {
                return Guid.Empty;
            }

            throw new ApiErrorException(
                "External user ID is missing from the current session.",
                errorCode: (int)HttpStatusCode.Unauthorized);
        }

        var tenantDbContext = serviceProvider.GetRequiredService<TenantDbContext>();
        var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        var cacheKey = $"UserId:{externalUserId}";

        //short‑circuit if it is already being created
        if (TryResolvePendingUserId(tenantDbContext, externalUserId, out var pendingUserId))
        {
            memoryCache.Set(cacheKey, pendingUserId, UserIdCacheDuration);
            return pendingUserId;
        }

        if (memoryCache.TryGetValue<Guid>(cacheKey, out var cachedUserId) &&
            IsCachedUserStillValid(tenantDbContext, cachedUserId, externalUserId))
        {
            return cachedUserId;
        }

        memoryCache.Remove(cacheKey);

        var userName = claimService.GetName();
        var email = claimService.GetEmail();
        var userId = tenantDbContext.EnsureUser(externalUserId, userName, email).GetAwaiter().GetResult();
        memoryCache.Set(cacheKey, userId, UserIdCacheDuration);

        return userId;
    }

    private static bool IsCachedUserStillValid(
        TenantDbContext tenantDbContext,
        Guid userId,
        string externalUserId)
    {
        return tenantDbContext.Users
            .AsNoTracking()
            .Any(entity => entity.Id == userId && entity.ExternalId == externalUserId);
    }

    private static bool TryResolvePendingUserId(
        TenantDbContext tenantDbContext,
        string externalUserId,
        out Guid userId)
    {
        userId = tenantDbContext.ChangeTracker
            .Entries<User>()
            .FirstOrDefault(entry =>
                entry.State == EntityState.Added &&
                entry.Entity.ExternalId == externalUserId)
            ?.Entity.Id ?? Guid.Empty;

        return userId != Guid.Empty;
    }
}
