using Querify.Common.EntityFramework.Tenant;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Tenant.Dtos.Tenant;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tenant.Portal.Business.Tenant.Queries.GetAllTenants;

public class TenantsGetAllTenantsQueryHandler(TenantDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TenantsGetAllTenantsQuery, List<TenantSummaryDto>>
{
    public async Task<List<TenantSummaryDto>> Handle(TenantsGetAllTenantsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = sessionService.GetUserId();

        return await dbContext.TenantUsers
            .AsNoTracking()
            .Where(entity => entity.UserId == userId && entity.Tenant.IsActive)
            .OrderBy(entity => entity.Tenant.Module)
            .ThenBy(entity => entity.Tenant.Name)
            .Select(entity => new TenantSummaryDto
            {
                Id = entity.TenantId,
                Slug = entity.Tenant.Slug,
                Name = entity.Tenant.Name,
                Edition = entity.Tenant.Edition,
                Module = entity.Tenant.Module,
                IsActive = entity.Tenant.IsActive,
                CurrentUserRole = entity.Role
            })
            .ToListAsync(cancellationToken);
    }
}
