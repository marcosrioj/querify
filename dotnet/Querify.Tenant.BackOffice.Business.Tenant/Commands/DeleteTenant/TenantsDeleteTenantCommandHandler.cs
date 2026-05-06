using Querify.Common.EntityFramework.Tenant;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Core.Helpers;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Querify.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenant;

public class TenantsDeleteTenantCommandHandler(
    TenantDbContext dbContext,
    IAllowedTenantStore allowedTenantStore)
    : IRequestHandler<TenantsDeleteTenantCommand>
{
    public async Task Handle(TenantsDeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenantUserIds = await dbContext.TenantUsers
            .Where(entity => entity.TenantId == request.Id)
            .Select(entity => entity.UserId)
            .ToListAsync(cancellationToken);

        var tenantUsers = await dbContext.TenantUsers
            .Where(entity => entity.TenantId == request.Id)
            .ToListAsync(cancellationToken);

        foreach (var tenantUser in tenantUsers)
        {
            dbContext.TenantUsers.Remove(tenantUser);
        }

        if (tenantUsers.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        dbContext.ChangeTracker.Clear();

        var tenant = await dbContext.Tenants
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (tenant is null)
        {
            throw new ApiErrorException(
                $"Tenant '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        dbContext.Tenants.Remove(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AllowedTenantCacheHelper.RemoveUserEntries(
            allowedTenantStore,
            tenantUserIds,
            cancellationToken);
    }
}
