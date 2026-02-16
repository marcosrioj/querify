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
        if (request.AiProviderId == Guid.Empty)
        {
            throw new ApiErrorException(
                "AiProviderId is required.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        var providerExists = await dbContext.AiProviders
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.AiProviderId, cancellationToken);

        if (!providerExists)
        {
            throw new ApiErrorException(
                $"AI Provider '{request.AiProviderId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

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
        tenant.AiProviderId = request.AiProviderId;
        tenant.IsActive = request.IsActive;
        tenant.UserId = request.UserId;

        dbContext.Tenants.Update(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}