using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Querify.Common.EntityFramework.Tenant;
using Querify.Models.Common.Enums;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.ExpirePendingSourceUploads;

namespace Querify.QnA.Worker.Business.Source.Commands.ExpirePendingSourceUploadsForAllTenants;

public sealed class ExpirePendingSourceUploadsForAllTenantsCommandHandler(
    TenantDbContext tenantDbContext,
    IServiceScopeFactory scopeFactory)
    : IRequestHandler<ExpirePendingSourceUploadsForAllTenantsCommand, bool>
{
    public async Task<bool> Handle(
        ExpirePendingSourceUploadsForAllTenantsCommand request,
        CancellationToken cancellationToken)
    {
        var tenantIds = await tenantDbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.Module == ModuleEnum.QnA && tenant.IsActive)
            .OrderBy(tenant => tenant.Id)
            .Select(tenant => tenant.Id)
            .ToListAsync(cancellationToken);

        var expiredAny = false;
        foreach (var tenantId in tenantIds)
        {
            await using var tenantScope = scopeFactory.CreateAsyncScope();
            var tenantContext = tenantScope.ServiceProvider.GetRequiredService<IQnAWorkerTenantContext>();
            using var activeTenant = tenantContext.UseTenant(tenantId);
            var mediator = tenantScope.ServiceProvider.GetRequiredService<IMediator>();
            expiredAny |= await mediator.Send(
                new ExpirePendingSourceUploadsCommand { NowUtc = request.NowUtc },
                cancellationToken);
        }

        return expiredAny;
    }
}
