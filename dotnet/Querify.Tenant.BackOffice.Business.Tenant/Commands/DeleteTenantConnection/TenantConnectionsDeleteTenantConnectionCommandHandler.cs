using Querify.Common.EntityFramework.Tenant;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Querify.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantConnection;

public class TenantConnectionsDeleteTenantConnectionCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantConnectionsDeleteTenantConnectionCommand>
{
    public async Task Handle(TenantConnectionsDeleteTenantConnectionCommand request,
        CancellationToken cancellationToken)
    {
        var tenantConnection = await dbContext.TenantConnections
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (tenantConnection is null)
        {
            throw new ApiErrorException(
                $"Tenant connection '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        dbContext.TenantConnections.Remove(tenantConnection);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}