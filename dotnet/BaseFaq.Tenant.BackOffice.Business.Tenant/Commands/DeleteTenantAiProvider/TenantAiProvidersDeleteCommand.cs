using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantAiProvider;

public sealed class TenantAiProvidersDeleteCommand : IRequest
{
    public required Guid Id { get; set; }
}