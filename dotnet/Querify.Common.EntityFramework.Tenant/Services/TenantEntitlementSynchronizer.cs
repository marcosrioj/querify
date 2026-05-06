using Querify.Common.EntityFramework.Tenant.Abstractions;
using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Models.Tenant.Enums;
using Microsoft.EntityFrameworkCore;

namespace Querify.Common.EntityFramework.Tenant.Services;

public sealed class TenantEntitlementSynchronizer(TenantDbContext dbContext) : ITenantEntitlementSynchronizer
{
    public async Task<TenantEntitlementSnapshot> SynchronizeAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await dbContext.TenantSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.TenantId == tenantId, cancellationToken);

        var snapshot = await dbContext.TenantEntitlementSnapshots
            .FirstOrDefaultAsync(entry => entry.TenantId == tenantId, cancellationToken);

        var state = ComputeState(subscription);

        if (snapshot is null)
        {
            snapshot = new TenantEntitlementSnapshot
            {
                TenantId = tenantId
            };

            await dbContext.TenantEntitlementSnapshots.AddAsync(snapshot, cancellationToken);
        }

        snapshot.PlanCode = state.PlanCode;
        snapshot.SubscriptionStatus = state.SubscriptionStatus;
        snapshot.IsActive = state.IsActive;
        snapshot.IsInGracePeriod = state.IsInGracePeriod;
        snapshot.EffectiveUntilUtc = state.EffectiveUntilUtc;

        await dbContext.SaveChangesAsync(cancellationToken);

        return snapshot;
    }

    private static EntitlementState ComputeState(TenantSubscription? subscription)
    {
        if (subscription is null)
        {
            return EntitlementState.Empty;
        }

        var now = DateTime.UtcNow;
        var effectiveUntilUtc = ResolveEffectiveUntil(subscription);
        var isInGracePeriod = subscription.Status is TenantSubscriptionStatus.PastDue or TenantSubscriptionStatus.Unpaid &&
                              effectiveUntilUtc.HasValue &&
                              effectiveUntilUtc.Value > now;

        var isActive = subscription.Status is TenantSubscriptionStatus.Active or TenantSubscriptionStatus.Trialing ||
                       isInGracePeriod ||
                       (subscription.Status == TenantSubscriptionStatus.Canceled &&
                        effectiveUntilUtc.HasValue &&
                        effectiveUntilUtc.Value > now);

        return new EntitlementState(
            subscription.PlanCode,
            subscription.Status,
            isActive,
            isInGracePeriod,
            effectiveUntilUtc);
    }

    private static DateTime? ResolveEffectiveUntil(TenantSubscription subscription)
    {
        return subscription.Status switch
        {
            TenantSubscriptionStatus.Trialing => subscription.TrialEndsAtUtc ?? subscription.CurrentPeriodEndUtc,
            TenantSubscriptionStatus.Active => subscription.CurrentPeriodEndUtc ?? subscription.TrialEndsAtUtc,
            TenantSubscriptionStatus.PastDue => subscription.GraceUntilUtc ?? subscription.CurrentPeriodEndUtc,
            TenantSubscriptionStatus.Unpaid => subscription.GraceUntilUtc ?? subscription.CurrentPeriodEndUtc,
            TenantSubscriptionStatus.Canceled => subscription.GraceUntilUtc ??
                                                 subscription.CurrentPeriodEndUtc ??
                                                 subscription.CancelledAtUtc,
            _ => subscription.CurrentPeriodEndUtc ?? subscription.TrialEndsAtUtc
        };
    }

    private sealed record EntitlementState(
        string? PlanCode,
        TenantSubscriptionStatus SubscriptionStatus,
        bool IsActive,
        bool IsInGracePeriod,
        DateTime? EffectiveUntilUtc)
    {
        public static EntitlementState Empty { get; } =
            new(null, TenantSubscriptionStatus.Unknown, false, false, null);
    }
}
