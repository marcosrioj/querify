using BaseFaq.Models.Tenant.Dtos.AiProvider;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Queries.GetAiProvider;

public sealed class AiProvidersGetAiProviderQuery : IRequest<AiProviderDto?>
{
    public required Guid Id { get; set; }
}