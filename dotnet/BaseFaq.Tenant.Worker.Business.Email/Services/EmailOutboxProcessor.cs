using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Common.EntityFramework.Tenant.Enums;
using BaseFaq.Common.EntityFramework.Tenant.Extensions;
using BaseFaq.Tenant.Worker.Business.Email.Abstractions;
using BaseFaq.Tenant.Worker.Business.Email.Commands.SendEmailOutbox;
using BaseFaq.Tenant.Worker.Business.Email.Infrastructure;
using BaseFaq.Tenant.Worker.Business.Email.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BaseFaq.Tenant.Worker.Business.Email.Services;

public sealed class EmailOutboxProcessor(
    TenantDbContext dbContext,
    IMediator mediator,
    IOptionsMonitor<EmailProcessingOptions> optionsMonitor,
    ILogger<EmailOutboxProcessor> logger)
    : IEmailOutboxProcessor
{
    private const string TableName = "EmailOutboxes";

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        var options = optionsMonitor.CurrentValue;

        if (!await dbContext.TableExistsAsync(TableName, cancellationToken))
        {
            logger.LogDebug(
                "Skipping email outbox polling because table {TableName} does not exist yet. Apply the TenantDb migration before enabling this processor.",
                TableName);
            return 0;
        }

        using var activity = EmailWorkerTelemetry.ActivitySource.StartActivity("email-outbox.process-batch");
        activity?.SetTag("worker.processor", "email-outbox");
        activity?.SetTag("worker.batch_size", options.BatchSize);

        var claimedItems = await ClaimBatchAsync(options, cancellationToken);
        if (claimedItems.Count == 0)
        {
            return 0;
        }

        logger.LogInformation(
            "Claimed {ClaimedCount} email outbox record(s) for processing.",
            claimedItems.Count);

        foreach (var item in claimedItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            WorkItemExecutionResult result;
            try
            {
                await mediator.Send(new SendEmailOutboxCommand(item), cancellationToken);
                result = WorkItemExecutionResult.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unhandled exception while sending email outbox record {EmailOutboxId} to {RecipientEmail}.",
                    item.Id,
                    item.RecipientEmail);

                result = WorkItemExecutionResult.Retry(ex.Message, options.FailureBackoff);
            }

            await FinalizeItemAsync(item, result, options, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Completed email outbox record {EmailOutboxId} for {RecipientEmail}.",
                    item.Id,
                    item.RecipientEmail);
                continue;
            }

            logger.LogWarning(
                "Email outbox record {EmailOutboxId} was not completed. Retry={ShouldRetry}; Reason={Reason}.",
                item.Id,
                result.ShouldRetry,
                result.FailureReason);
        }

        return claimedItems.Count;
    }

    private async Task<List<EmailOutbox>> ClaimBatchAsync(
        EmailProcessingOptions options,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var candidateIds = await dbContext.EmailOutboxes
            .AsNoTracking()
            .Where(item =>
                item.Status == ControlPlaneMessageStatus.Pending &&
                item.ProcessedDateUtc == null &&
                (item.NextAttemptDateUtc == null || item.NextAttemptDateUtc <= now) &&
                (item.LockedUntilDateUtc == null || item.LockedUntilDateUtc <= now))
            .OrderBy(item => item.QueuedDateUtc)
            .Select(item => item.Id)
            .Take(options.BatchSize * 4)
            .ToListAsync(cancellationToken);

        var claimedItems = new List<EmailOutbox>(options.BatchSize);

        foreach (var candidateId in candidateIds)
        {
            if (claimedItems.Count >= options.BatchSize)
            {
                break;
            }

            var processingToken = Guid.NewGuid();
            var leaseUntil = now.Add(options.LeaseDuration);

            var updatedRows = await dbContext.EmailOutboxes
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

            var claimedItem = await dbContext.EmailOutboxes
                .AsNoTracking()
                .SingleAsync(
                    item => item.Id == candidateId && item.ProcessingToken == processingToken,
                    cancellationToken);

            claimedItems.Add(claimedItem);
        }

        return claimedItems;
    }

    private async Task FinalizeItemAsync(
        EmailOutbox item,
        WorkItemExecutionResult result,
        EmailProcessingOptions options,
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

        await dbContext.EmailOutboxes
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
