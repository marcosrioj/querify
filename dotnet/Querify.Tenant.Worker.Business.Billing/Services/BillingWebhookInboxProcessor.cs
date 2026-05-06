using Querify.Common.EntityFramework.Tenant;
using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Common.EntityFramework.Tenant.Enums;
using Querify.Common.EntityFramework.Tenant.Extensions;
using Querify.Tenant.Worker.Business.Billing.Abstractions;
using Querify.Tenant.Worker.Business.Billing.Commands.DispatchBillingWebhookInbox;
using Querify.Tenant.Worker.Business.Billing.Infrastructure;
using Querify.Tenant.Worker.Business.Billing.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Querify.Tenant.Worker.Business.Billing.Services;

public sealed class BillingWebhookInboxProcessor(
    TenantDbContext dbContext,
    IMediator mediator,
    IOptionsMonitor<BillingProcessingOptions> optionsMonitor,
    ILogger<BillingWebhookInboxProcessor> logger)
    : IBillingWebhookInboxProcessor
{
    private const string TableName = "BillingWebhookInboxes";

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        var options = optionsMonitor.CurrentValue;

        if (!await dbContext.TableExistsAsync(TableName, cancellationToken))
        {
            logger.LogDebug(
                "Skipping billing webhook inbox polling because table {TableName} does not exist yet. Apply the TenantDb migration before enabling this processor.",
                TableName);
            return 0;
        }

        using var activity = BillingWorkerTelemetry.ActivitySource.StartActivity("billing-webhook-inbox.process-batch");
        activity?.SetTag("worker.processor", "billing-webhook-inbox");
        activity?.SetTag("worker.batch_size", options.BatchSize);

        var claimedItems = await ClaimBatchAsync(options, cancellationToken);
        if (claimedItems.Count == 0)
        {
            return 0;
        }

        logger.LogInformation(
            "Claimed {ClaimedCount} billing webhook inbox record(s) for processing.",
            claimedItems.Count);

        foreach (var item in claimedItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            WorkItemExecutionResult result;
            try
            {
                await mediator.Send(new DispatchBillingWebhookInboxCommand(item), cancellationToken);
                result = WorkItemExecutionResult.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unhandled exception while dispatching billing webhook inbox record {BillingWebhookInboxId} for provider {Provider} and event {EventType}.",
                    item.Id,
                    item.Provider,
                    item.EventType);

                result = WorkItemExecutionResult.Retry(ex.Message, options.FailureBackoff);
            }

            await FinalizeItemAsync(item, result, options, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Completed billing webhook inbox record {BillingWebhookInboxId} for provider {Provider} and event {EventType}.",
                    item.Id,
                    item.Provider,
                    item.EventType);
                continue;
            }

            logger.LogWarning(
                "Billing webhook inbox record {BillingWebhookInboxId} was not completed. Retry={ShouldRetry}; Reason={Reason}.",
                item.Id,
                result.ShouldRetry,
                result.FailureReason);
        }

        return claimedItems.Count;
    }

    private async Task<List<BillingWebhookInbox>> ClaimBatchAsync(
        BillingProcessingOptions options,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var candidateIds = await dbContext.BillingWebhookInboxes
            .AsNoTracking()
            .Where(item =>
                item.Status == ControlPlaneMessageStatus.Pending &&
                item.ProcessedDateUtc == null &&
                (item.NextAttemptDateUtc == null || item.NextAttemptDateUtc <= now) &&
                (item.LockedUntilDateUtc == null || item.LockedUntilDateUtc <= now))
            .OrderBy(item => item.ReceivedDateUtc)
            .Select(item => item.Id)
            .Take(options.BatchSize * 4)
            .ToListAsync(cancellationToken);

        var claimedItems = new List<BillingWebhookInbox>(options.BatchSize);

        foreach (var candidateId in candidateIds)
        {
            if (claimedItems.Count >= options.BatchSize)
            {
                break;
            }

            var processingToken = Guid.NewGuid();
            var leaseUntil = now.Add(options.LeaseDuration);

            var updatedRows = await dbContext.BillingWebhookInboxes
                .Where(item =>
                    item.Id == candidateId &&
                    item.Status == ControlPlaneMessageStatus.Pending &&
                    item.ProcessedDateUtc == null &&
                    (item.NextAttemptDateUtc == null || item.NextAttemptDateUtc <= now) &&
                    (item.LockedUntilDateUtc == null || item.LockedUntilDateUtc <= now))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.Status, ControlPlaneMessageStatus.Processing)
                    .SetProperty(item => item.LockedUntilDateUtc, leaseUntil)
                    .SetProperty(item => item.ProcessingToken, processingToken)
                    .SetProperty(item => item.LastAttemptDateUtc, now)
                    .SetProperty(item => item.AttemptCount, item => item.AttemptCount + 1)
                    .SetProperty(item => item.LastError, (string?)null), cancellationToken);

            if (updatedRows == 0)
            {
                continue;
            }

            var claimedItem = await dbContext.BillingWebhookInboxes
                .AsNoTracking()
                .SingleAsync(
                    item => item.Id == candidateId && item.ProcessingToken == processingToken,
                    cancellationToken);

            claimedItems.Add(claimedItem);
        }

        return claimedItems;
    }

    private async Task FinalizeItemAsync(
        BillingWebhookInbox item,
        WorkItemExecutionResult result,
        BillingProcessingOptions options,
        CancellationToken cancellationToken)
    {
        var completedAt = DateTime.UtcNow;
        var isTerminalFailure = !result.IsSuccess &&
                                (!result.ShouldRetry || item.AttemptCount >= options.MaxRetryCount);
        var nextAttemptDateUtc = result.IsSuccess || isTerminalFailure
            ? (DateTime?)null
            : completedAt.Add(result.RetryAfter ?? options.FailureBackoff);
        var targetStatus = result.IsSuccess
            ? ControlPlaneMessageStatus.Completed
            : isTerminalFailure
                ? ControlPlaneMessageStatus.Failed
                : ControlPlaneMessageStatus.Pending;

        await dbContext.BillingWebhookInboxes
            .Where(entry => entry.Id == item.Id && entry.ProcessingToken == item.ProcessingToken)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entry => entry.Status, targetStatus)
                .SetProperty(entry => entry.ProcessedDateUtc, result.IsSuccess ? completedAt : (DateTime?)null)
                .SetProperty(entry => entry.NextAttemptDateUtc, nextAttemptDateUtc)
                .SetProperty(entry => entry.LockedUntilDateUtc, (DateTime?)null)
                .SetProperty(entry => entry.ProcessingToken, (Guid?)null)
                .SetProperty(entry => entry.LastError, result.IsSuccess ? (string?)null : result.FailureReason),
                cancellationToken);
    }
}
