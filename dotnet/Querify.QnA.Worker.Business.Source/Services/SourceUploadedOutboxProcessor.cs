using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Querify.Common.EntityFramework.Tenant;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.IntegrationEvents;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Infrastructure;
using Querify.QnA.Worker.Business.Source.Options;

namespace Querify.QnA.Worker.Business.Source.Services;

public sealed class SourceUploadedOutboxProcessor(
    TenantDbContext tenantDbContext,
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<SourceUploadedOutboxProcessingOptions> optionsMonitor,
    ILogger<SourceUploadedOutboxProcessor> logger)
    : ISourceUploadedOutboxProcessor
{
    private const string TableName = "SourceUploadedOutboxMessages";

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        var tenantIds = await tenantDbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.Module == ModuleEnum.QnA && tenant.IsActive)
            .OrderBy(tenant => tenant.Id)
            .Select(tenant => tenant.Id)
            .ToListAsync(cancellationToken);

        var processedCount = 0;
        foreach (var tenantId in tenantIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            processedCount += await ProcessTenantBatchAsync(tenantId, cancellationToken);

            if (processedCount >= optionsMonitor.CurrentValue.BatchSize)
            {
                break;
            }
        }

        return processedCount;
    }

    private async Task<int> ProcessTenantBatchAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var tenantContext = scope.ServiceProvider.GetRequiredService<IQnAWorkerTenantContext>();
        using var tenantScope = tenantContext.UseTenant(tenantId);
        var dbContext = scope.ServiceProvider.GetRequiredService<QnADbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        if (!await dbContext.TableExistsAsync(TableName, cancellationToken))
        {
            logger.LogDebug(
                "Skipping source upload outbox polling for tenant {TenantId} because table {TableName} does not exist yet.",
                tenantId,
                TableName);
            return 0;
        }

        var options = optionsMonitor.CurrentValue;
        var claimedMessages = await ClaimBatchAsync(dbContext, options, cancellationToken);
        foreach (var message in claimedMessages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await publishEndpoint.Publish(new SourceUploadedIntegrationEvent
                {
                    TenantId = message.TenantId,
                    SourceId = message.SourceId,
                    StorageKey = message.StorageKey,
                    ClientChecksum = message.ClientChecksum,
                    UploadedAtUtc = message.UploadedAtUtc
                }, cancellationToken);

                await FinalizeMessageAsync(dbContext, message, success: true, null, options, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to publish source upload outbox message {OutboxMessageId} for tenant {TenantId}.",
                    message.Id,
                    tenantId);

                await FinalizeMessageAsync(dbContext, message, success: false, ex.Message, options,
                    cancellationToken);
            }
        }

        return claimedMessages.Count;
    }

    private static async Task<List<Common.Domain.Entities.SourceUploadedOutboxMessage>> ClaimBatchAsync(
        QnADbContext dbContext,
        SourceUploadedOutboxProcessingOptions options,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var candidateIds = await dbContext.SourceUploadedOutboxMessages
            .AsNoTracking()
            .Where(message =>
                message.Status == SourceUploadOutboxStatus.Pending &&
                message.ProcessedDateUtc == null &&
                (message.NextAttemptDateUtc == null || message.NextAttemptDateUtc <= now) &&
                (message.LockedUntilDateUtc == null || message.LockedUntilDateUtc <= now))
            .OrderBy(message => message.UploadedAtUtc)
            .Select(message => message.Id)
            .Take(options.BatchSize * 4)
            .ToListAsync(cancellationToken);

        var claimedMessages = new List<Common.Domain.Entities.SourceUploadedOutboxMessage>(options.BatchSize);

        foreach (var candidateId in candidateIds)
        {
            if (claimedMessages.Count >= options.BatchSize)
            {
                break;
            }

            var processingToken = Guid.NewGuid();
            var leaseUntil = now.Add(options.LeaseDuration);

            var updatedRows = await dbContext.SourceUploadedOutboxMessages
                .Where(message =>
                    message.Id == candidateId &&
                    message.Status == SourceUploadOutboxStatus.Pending &&
                    message.ProcessedDateUtc == null &&
                    (message.NextAttemptDateUtc == null || message.NextAttemptDateUtc <= now) &&
                    (message.LockedUntilDateUtc == null || message.LockedUntilDateUtc <= now))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(message => message.Status, SourceUploadOutboxStatus.Processing)
                    .SetProperty(message => message.LockedUntilDateUtc, leaseUntil)
                    .SetProperty(message => message.ProcessingToken, processingToken)
                    .SetProperty(message => message.LastAttemptDateUtc, now)
                    .SetProperty(message => message.AttemptCount, message => message.AttemptCount + 1)
                    .SetProperty(message => message.LastError, (string?)null), cancellationToken);

            if (updatedRows == 0)
            {
                continue;
            }

            var claimed = await dbContext.SourceUploadedOutboxMessages
                .AsNoTracking()
                .SingleAsync(message => message.Id == candidateId &&
                                        message.ProcessingToken == processingToken, cancellationToken);

            claimedMessages.Add(claimed);
        }

        return claimedMessages;
    }

    private static async Task FinalizeMessageAsync(
        QnADbContext dbContext,
        Common.Domain.Entities.SourceUploadedOutboxMessage message,
        bool success,
        string? failureReason,
        SourceUploadedOutboxProcessingOptions options,
        CancellationToken cancellationToken)
    {
        var completedAt = DateTime.UtcNow;
        var isTerminalFailure = !success && message.AttemptCount >= options.MaxRetryCount;
        var targetStatus = success
            ? SourceUploadOutboxStatus.Completed
            : isTerminalFailure
                ? SourceUploadOutboxStatus.Failed
                : SourceUploadOutboxStatus.Pending;
        var nextAttemptDateUtc = success || isTerminalFailure
            ? (DateTime?)null
            : completedAt.Add(options.FailureBackoff);

        await dbContext.SourceUploadedOutboxMessages
            .Where(entry => entry.Id == message.Id && entry.ProcessingToken == message.ProcessingToken)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entry => entry.Status, targetStatus)
                .SetProperty(entry => entry.ProcessedDateUtc, success ? completedAt : (DateTime?)null)
                .SetProperty(entry => entry.NextAttemptDateUtc, nextAttemptDateUtc)
                .SetProperty(entry => entry.LockedUntilDateUtc, (DateTime?)null)
                .SetProperty(entry => entry.ProcessingToken, (Guid?)null)
                .SetProperty(entry => entry.LastError, success ? (string?)null : failureReason),
                cancellationToken);
    }
}
