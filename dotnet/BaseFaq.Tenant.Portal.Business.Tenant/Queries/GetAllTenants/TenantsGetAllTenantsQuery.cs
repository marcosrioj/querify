using BaseFaq.Models.Tenant.Dtos.Tenant;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetAllTenants;

public sealed class TenantsGetAllTenantsQuery : IRequest<List<TenantSummaryDto>>;