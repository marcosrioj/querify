using Querify.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using Querify.Direct.Common.Persistence.DirectDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace Querify.Direct.Common.Persistence.DirectDb.DbContext.TenantIntegrity;

internal static class ConversationMessageTenantIntegrityExtension
{
    internal static void EnsureConversationMessageTenantIntegrity(
        this DirectDbContext dbContext,
        TenantIntegrityLookupCacheBase cacheBase)
    {
        Dictionary<Guid, Guid>? conversationTenants = null;

        foreach (var entry in dbContext.ChangeTracker.Entries<ConversationMessage>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var message = entry.Entity;
            TenantIntegrityGuard.EnsureTenantMatch(
                message.TenantId,
                cacheBase.GetTenant<Conversation>(message.ConversationId, ref conversationTenants),
                nameof(ConversationMessage.ConversationId));
        }
    }
}
