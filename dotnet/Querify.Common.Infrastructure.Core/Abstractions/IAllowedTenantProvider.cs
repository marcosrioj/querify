namespace Querify.Common.Infrastructure.Core.Abstractions;

public interface IAllowedTenantProvider
{
    Task<IReadOnlyDictionary<string, IReadOnlyCollection<Guid>>> GetAllowedTenantIds(Guid userId,
        CancellationToken cancellationToken = default);
}