using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Tenant.Enums;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Common.EntityFramework.Tenant.Providers;

public sealed class TenantAiProviderResolver(TenantDbContext tenantDbContext) : ITenantAiProviderResolver
{
    public Task<bool> HasProviderForCommandAsync(
        Guid tenantId,
        AiCommandType commandType,
        CancellationToken cancellationToken = default)
    {
        return tenantDbContext.TenantAiProviders
            .AsNoTracking()
            .Include(x => x.AiProvider)
            .AnyAsync(
                x => x.TenantId == tenantId && x.AiProvider.Command == commandType,
                cancellationToken);
    }
}