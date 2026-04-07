using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Helpers;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenant;

public class TenantsUpdateTenantCommandHandler(
    TenantDbContext dbContext,
    IAllowedTenantStore allowedTenantStore)
    : IRequestHandler<TenantsUpdateTenantCommand>
{
    public async Task Handle(TenantsUpdateTenantCommand request, CancellationToken cancellationToken)
    {
        await EnsureOwnerExistsAsync(request.UserId, cancellationToken);

        var tenant = await dbContext.Tenants
            .Include(entity => entity.TenantUsers)
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (tenant is null)
        {
            throw new ApiErrorException(
                $"Tenant '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        var previousOwnerUserId = TenantUserHelper.GetOwnerUserId(tenant.TenantUsers);

        tenant.Slug = request.Slug;
        tenant.Name = request.Name;
        tenant.Edition = request.Edition;
        tenant.ConnectionString = request.ConnectionString;
        tenant.IsActive = request.IsActive;
        TenantUserHelper.SetOwner(tenant.TenantUsers, tenant.Id, request.UserId);

        dbContext.Tenants.Update(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);

        await AllowedTenantCacheHelper.RemoveUserEntries(
            allowedTenantStore,
            [request.UserId, previousOwnerUserId ?? Guid.Empty],
            cancellationToken);
    }

    private async Task EnsureOwnerExistsAsync(Guid ownerUserId, CancellationToken cancellationToken)
    {
        if (await dbContext.Users.AsNoTracking().AnyAsync(user => user.Id == ownerUserId, cancellationToken))
        {
            return;
        }

        throw new ApiErrorException(
            $"Owner user '{ownerUserId}' was not found.",
            errorCode: (int)HttpStatusCode.NotFound);
    }
}
