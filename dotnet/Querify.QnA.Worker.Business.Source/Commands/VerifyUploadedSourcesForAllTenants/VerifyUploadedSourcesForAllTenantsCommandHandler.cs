using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Querify.Common.EntityFramework.Tenant;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Sources;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource;

namespace Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSourcesForAllTenants;

public sealed class VerifyUploadedSourcesForAllTenantsCommandHandler(
    TenantDbContext tenantDbContext,
    IServiceScopeFactory scopeFactory)
    : IRequestHandler<VerifyUploadedSourcesForAllTenantsCommand, bool>
{
    public async Task<bool> Handle(
        VerifyUploadedSourcesForAllTenantsCommand request,
        CancellationToken cancellationToken)
    {
        var remaining = Math.Max(1, request.BatchSize);
        var tenantIds = await tenantDbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.Module == ModuleEnum.QnA && tenant.IsActive)
            .OrderBy(tenant => tenant.Id)
            .Select(tenant => tenant.Id)
            .ToListAsync(cancellationToken);

        var verifiedAny = false;
        foreach (var tenantId in tenantIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var processedCount = await VerifyTenantUploadedSourcesAsync(tenantId, remaining, cancellationToken);
            verifiedAny |= processedCount > 0;
            remaining -= processedCount;

            if (remaining <= 0)
            {
                break;
            }
        }

        return verifiedAny;
    }

    private async Task<int> VerifyTenantUploadedSourcesAsync(
        Guid tenantId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        await using var tenantScope = scopeFactory.CreateAsyncScope();
        var tenantContext = tenantScope.ServiceProvider.GetRequiredService<IQnAWorkerTenantContext>();
        using var activeTenant = tenantContext.UseTenant(tenantId);
        var dbContext = tenantScope.ServiceProvider.GetRequiredService<QnADbContext>();
        var mediator = tenantScope.ServiceProvider.GetRequiredService<IMediator>();

        var candidates = await dbContext.Sources
            .AsNoTracking()
            .Where(source => source.UploadStatus == SourceUploadStatus.Uploaded &&
                             source.StorageKey != null)
            .OrderBy(source => source.Id)
            .Select(source => new UploadedSourceCandidate(source.Id, source.StorageKey!))
            .Take(batchSize * 4)
            .ToListAsync(cancellationToken);

        var processedCount = 0;
        foreach (var candidate in candidates)
        {
            if (processedCount >= batchSize)
            {
                break;
            }

            if (!SourceStorageKey.IsStagingKey(candidate.StorageKey))
            {
                continue;
            }

            await mediator.Send(new VerifyUploadedSourceCommand
            {
                TenantId = tenantId,
                SourceId = candidate.SourceId,
                StorageKey = candidate.StorageKey
            }, cancellationToken);

            processedCount++;
        }

        return processedCount;
    }

    private sealed record UploadedSourceCandidate(Guid SourceId, string StorageKey);
}
