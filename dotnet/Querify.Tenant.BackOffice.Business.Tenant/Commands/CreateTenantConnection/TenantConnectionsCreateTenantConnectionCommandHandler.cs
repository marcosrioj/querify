using Querify.Common.EntityFramework.Tenant;
using Querify.Common.EntityFramework.Tenant.Entities;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Commands.CreateTenantConnection;

public class TenantConnectionsCreateTenantConnectionCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantConnectionsCreateTenantConnectionCommand, Guid>
{
    public async Task<Guid> Handle(TenantConnectionsCreateTenantConnectionCommand request,
        CancellationToken cancellationToken)
    {
        var connection = new TenantConnection
        {
            Module = request.Module,
            ConnectionString = request.ConnectionString,
            IsCurrent = request.IsCurrent
        };

        await dbContext.TenantConnections.AddAsync(connection, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return connection.Id;
    }
}