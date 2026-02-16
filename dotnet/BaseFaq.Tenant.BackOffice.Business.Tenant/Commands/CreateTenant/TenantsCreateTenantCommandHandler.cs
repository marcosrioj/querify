using BaseFaq.Common.EntityFramework.Tenant;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenant;

public class TenantsCreateTenantCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantsCreateTenantCommand, Guid>
{
    public async Task<Guid> Handle(TenantsCreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = new BaseFaq.Common.EntityFramework.Tenant.Entities.Tenant
        {
            Slug = request.Slug,
            Name = request.Name,
            Edition = request.Edition,
            App = request.App,
            ConnectionString = request.ConnectionString,
            IsActive = request.IsActive,
            UserId = request.UserId
        };

        await dbContext.Tenants.AddAsync(tenant, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}