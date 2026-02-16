using BaseFaq.Models.Tenant.Dtos.AiProvider;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.AiProvider.Queries.GetAiProviderList;

public class AiProvidersGetAiProviderListQuery : IRequest<List<AiProviderDto>>;