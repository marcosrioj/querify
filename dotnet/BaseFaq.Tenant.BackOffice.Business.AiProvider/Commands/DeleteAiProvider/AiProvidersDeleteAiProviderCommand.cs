using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Commands.DeleteAiProvider;

public sealed class AiProvidersDeleteAiProviderCommand : IRequest
{
    public required Guid Id { get; set; }
}