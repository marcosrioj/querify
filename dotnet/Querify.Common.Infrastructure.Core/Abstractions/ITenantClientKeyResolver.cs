namespace Querify.Common.Infrastructure.Core.Abstractions;

public interface ITenantClientKeyResolver
{
    Task<Guid> ResolveTenantId(string clientKey, CancellationToken cancellationToken = default);
}