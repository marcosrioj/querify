using BaseFaq.AI.Business.Common.Utilities;
using BaseFaq.AI.Business.Generation.Abstractions;
using BaseFaq.Models.Ai.Contracts.Generation;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BaseFaq.AI.Business.Generation.Commands.ProcessFaqGenerationRequested;

public sealed class ProcessFaqGenerationRequestedCommandHandler(
    IGenerationExecutionService generationExecutionService,
    ILogger<ProcessFaqGenerationRequestedCommandHandler> logger,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<ProcessFaqGenerationRequestedCommand>
{
    private const string GenerationErrorCode = "GENERATION_FAILED";
    private const int MaxErrorMessageLength = 4000;

    public async Task Handle(ProcessFaqGenerationRequestedCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Message);

        var message = command.Message;
        var jobId = Guid.NewGuid();

        try
        {
            await generationExecutionService.ExecuteAsync(message, cancellationToken);
            await PublishGenerationReadyAsync(message, jobId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Generation worker failed for CorrelationId {CorrelationId}, FaqId {FaqId}, TenantId {TenantId}.",
                message.CorrelationId,
                message.FaqId,
                message.TenantId);

            await PublishGenerationFailedSafeAsync(message, jobId, ex, cancellationToken);
        }
    }

    private async Task PublishGenerationReadyAsync(
        FaqGenerationRequestedV1 message,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new FaqGenerationReadyV1
        {
            EventId = Guid.NewGuid(),
            CorrelationId = message.CorrelationId,
            JobId = jobId,
            FaqId = message.FaqId,
            TenantId = message.TenantId,
            OccurredUtc = DateTime.UtcNow
        }, cancellationToken);
    }

    private async Task PublishGenerationFailedSafeAsync(
        FaqGenerationRequestedV1 message,
        Guid jobId,
        Exception ex,
        CancellationToken cancellationToken)
    {
        var errorMessage = TextBounds.Truncate(ex.Message, MaxErrorMessageLength);

        try
        {
            await publishEndpoint.Publish(new FaqGenerationFailedV1
            {
                EventId = Guid.NewGuid(),
                CorrelationId = message.CorrelationId,
                JobId = jobId,
                FaqId = message.FaqId,
                TenantId = message.TenantId,
                ErrorCode = GenerationErrorCode,
                ErrorMessage = errorMessage,
                OccurredUtc = DateTime.UtcNow
            }, cancellationToken);
        }
        catch (Exception publishEx)
        {
            logger.LogError(
                publishEx,
                "Failed to publish generation failure callback. CorrelationId {CorrelationId}, FaqId {FaqId}, TenantId {TenantId}.",
                message.CorrelationId,
                message.FaqId,
                message.TenantId);
        }
    }
}