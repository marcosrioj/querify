using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Tenant.Dtos.Tenant;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetAllTenants;

public class TenantsGetAllTenantsQueryHandler(TenantDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TenantsGetAllTenantsQuery, List<TenantSummaryDto>>
{
    public async Task<List<TenantSummaryDto>> Handle(TenantsGetAllTenantsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = sessionService.GetUserId();

        return await dbContext.Tenants
            .AsNoTracking()
            .Where(entity => entity.UserId == userId && entity.IsActive)
            .OrderBy(entity => entity.App)
            .Select(tenant => new TenantSummaryDto
            {
                Id = tenant.Id,
                Slug = tenant.Slug,
                Name = tenant.Name,
                Edition = tenant.Edition,
                App = tenant.App,
                IsActive = tenant.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}