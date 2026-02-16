using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenant;

public class TenantsUpdateTenantCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantsUpdateTenantCommand>
{
    public async Task Handle(TenantsUpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (tenant is null)
        {
            throw new ApiErrorException(
                $"Tenant '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        tenant.Slug = request.Slug;
        tenant.Name = request.Name;
        tenant.Edition = request.Edition;
        tenant.ConnectionString = request.ConnectionString;
        tenant.IsActive = request.IsActive;
        tenant.UserId = request.UserId;

        dbContext.Tenants.Update(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}