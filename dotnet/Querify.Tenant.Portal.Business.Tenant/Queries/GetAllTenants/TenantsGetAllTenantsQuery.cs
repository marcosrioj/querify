using Querify.Models.Tenant.Dtos.Tenant;
using MediatR;

namespace Querify.Tenant.Portal.Business.Tenant.Queries.GetAllTenants;

public sealed class TenantsGetAllTenantsQuery : IRequest<List<TenantSummaryDto>>;