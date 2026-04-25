using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Common.EntityFramework.Tenant.Helpers;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Helpers;
using BaseFaq.Models.Tenant.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenant;

public class TenantsCreateTenantCommandHandler(
    TenantDbContext dbContext,
    IAllowedTenantStore allowedTenantStore)
    : IRequestHandler<TenantsCreateTenantCommand, Guid>
{
    public async Task<Guid> Handle(TenantsCreateTenantCommand request, CancellationToken cancellationToken)
    {
        await EnsureOwnerExistsAsync(request.UserId, cancellationToken);

        var tenantId = Guid.NewGuid();
        var tenant = new Common.EntityFramework.Tenant.Entities.Tenant
        {
            Id = tenantId,
            Slug = request.Slug,
            Name = request.Name,
            Edition = request.Edition,
            Module = request.Module,
            ConnectionString = request.ConnectionString,
            IsActive = request.IsActive
        };
        TenantUserHelper.SetOwner(tenant.TenantUsers, tenantId, request.UserId);

        await dbContext.Tenants.AddAsync(tenant, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AllowedTenantCacheHelper.RemoveUserEntries(allowedTenantStore, [request.UserId], cancellationToken);

        return tenant.Id;
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
