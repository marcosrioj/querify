using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetClientKey;

public sealed class TenantsGetClientKeyQuery : IRequest<string?>;