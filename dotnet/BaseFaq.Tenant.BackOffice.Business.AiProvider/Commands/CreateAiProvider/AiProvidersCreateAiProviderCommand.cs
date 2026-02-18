using BaseFaq.Models.Tenant.Enums;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Commands.CreateAiProvider;

public sealed class AiProvidersCreateAiProviderCommand : IRequest<Guid>
{
    public required string Provider { get; set; }
    public required string Model { get; set; }
    public required string Prompt { get; set; }
    public required AiCommandType Command { get; set; }
}