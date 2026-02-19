using BaseFaq.AI.Business.Common.Abstractions;
using BaseFaq.AI.Business.Common.Models;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Tenant.Enums;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.AI.Business.Common.Infrastructure;

public sealed class AiProviderContextResolver(TenantDbContext tenantDbContext) : IAiProviderContextResolver
{
    public async Task<AiProviderContext> ResolveAsync(
        Guid tenantId,
        AiCommandType commandType,
        CancellationToken cancellationToken = default)
    {
        var provider = await tenantDbContext.TenantAiProviders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AiProvider.Command == commandType)
            .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate ?? DateTime.MinValue)
            .ThenByDescending(x => x.CreatedDate ?? DateTime.MinValue)
            .ThenBy(x => x.AiProvider.Provider)
            .ThenBy(x => x.AiProvider.Model)
            .Select(x => new AiProviderContext(
                x.AiProvider.Provider,
                x.AiProvider.Model,
                x.AiProvider.Prompt,
                x.AiProviderKey))
            .FirstOrDefaultAsync(cancellationToken);

        if (provider is null)
        {
            throw new InvalidOperationException(
                $"Tenant '{tenantId}' has no AI provider configured for '{commandType}'.");
        }

        return provider;
    }
}