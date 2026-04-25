using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Tenant.Dtos.TenantConnection;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantConnection;

public class TenantConnectionsGetTenantConnectionQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantConnectionsGetTenantConnectionQuery, TenantConnectionDto?>
{
    public async Task<TenantConnectionDto?> Handle(TenantConnectionsGetTenantConnectionQuery request,
        CancellationToken cancellationToken)
    {
        var connection = await dbContext.TenantConnections
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (connection is null)
        {
            return null;
        }

        return new TenantConnectionDto
        {
            Id = connection.Id,
            Module = connection.Module,
            ConnectionString = string.Empty,
            IsCurrent = connection.IsCurrent
        };
    }
}