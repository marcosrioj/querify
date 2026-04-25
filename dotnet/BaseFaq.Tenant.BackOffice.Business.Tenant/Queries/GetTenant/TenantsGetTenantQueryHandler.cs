using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Tenant.Dtos.Tenant;
using BaseFaq.Models.Tenant.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenant;

public class TenantsGetTenantQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantsGetTenantQuery, TenantDto?>
{
    public async Task<TenantDto?> Handle(TenantsGetTenantQuery request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .Include(entity => entity.TenantUsers)
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        return new TenantDto
        {
            Id = tenant.Id,
            Slug = tenant.Slug,
            Name = tenant.Name,
            Edition = tenant.Edition,
            Module = tenant.Module,
            ConnectionString = string.Empty,
            IsActive = tenant.IsActive,
            UserId = tenant.TenantUsers
                .Where(tenantUser => tenantUser.Role == TenantUserRoleType.Owner)
                .Select(tenantUser => tenantUser.UserId)
                .FirstOrDefault()
        };
    }
}
