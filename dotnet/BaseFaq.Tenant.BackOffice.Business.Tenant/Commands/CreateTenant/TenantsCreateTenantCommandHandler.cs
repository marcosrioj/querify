using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenant;

public class TenantsCreateTenantCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantsCreateTenantCommand, Guid>
{
    public async Task<Guid> Handle(TenantsCreateTenantCommand request, CancellationToken cancellationToken)
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

        var tenant = new BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant
        {
            Slug = request.Slug,
            Name = request.Name,
            Edition = request.Edition,
            App = request.App,
            ConnectionString = request.ConnectionString,
            AiProviderId = request.AiProviderId,
            IsActive = request.IsActive,
            UserId = request.UserId
        };

        await dbContext.Tenants.AddAsync(tenant, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}