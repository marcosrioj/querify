using BaseFaq.Common.Infrastructure.Core.Abstractions;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;

public sealed class TestTenantClientKeyResolver(Guid expectedTenantId, string expectedClientKey)
    : ITenantClientKeyResolver
{
    public Task<Guid> ResolveTenantId(string clientKey, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(clientKey, expectedClientKey, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Unexpected client key '{clientKey}'.");
        }

        return Task.FromResult(expectedTenantId);
    }
}
