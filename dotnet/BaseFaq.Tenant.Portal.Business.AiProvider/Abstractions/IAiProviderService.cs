using BaseFaq.Models.Tenant.Dtos.AiProvider;

namespace BaseFaq.Tenant.Portal.Business.AiProvider.Abstractions;

public interface IAiProviderService
{
    Task<List<AiProviderDto>> GetAll(CancellationToken token);
}