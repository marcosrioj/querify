using MediatR;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.IsAiProviderKeyConfigured;

public class TenantsIsAiProviderKeyConfiguredQuery : IRequest<bool>
{
    public required AiCommandType Command { get; set; }
}