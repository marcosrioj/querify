using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Faq.Public.Test.IntegrationTests.Helpers;

public sealed class TestTenantAiProviderResolver(bool hasProvider = true) : ITenantAiProviderResolver
{
    public Task<bool> HasProviderForCommandAsync(
        Guid tenantId,
        AiCommandType commandType,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(hasProvider);
    }
}