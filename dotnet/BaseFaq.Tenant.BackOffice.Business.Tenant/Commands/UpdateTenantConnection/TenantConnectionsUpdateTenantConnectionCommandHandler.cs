using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenantConnection;

public class TenantConnectionsUpdateTenantConnectionCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantConnectionsUpdateTenantConnectionCommand>
{
    public async Task Handle(TenantConnectionsUpdateTenantConnectionCommand request,
        CancellationToken cancellationToken)
    {
        var connection = await dbContext.TenantConnections
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (connection is null)
        {
            throw new ApiErrorException(
                $"Tenant connection '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        connection.Module = request.Module;
        connection.ConnectionString = request.ConnectionString;
        connection.IsCurrent = request.IsCurrent;

        dbContext.TenantConnections.Update(connection);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}