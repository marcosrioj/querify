using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantAiProvider;

public class TenantAiProvidersDeleteCommand : IRequest
{
    public required Guid Id { get; set; }
}