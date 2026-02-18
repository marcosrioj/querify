using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.GenerateNewClientKey;

public sealed class TenantsGenerateNewClientKeyCommand : IRequest<string>;