using BaseFaq.Common.EntityFramework.Tenant;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantAiProvider;

public class TenantAiProvidersDeleteCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantAiProvidersDeleteCommand>
{
    public async Task Handle(TenantAiProvidersDeleteCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TenantAiProviders.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        dbContext.TenantAiProviders.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}