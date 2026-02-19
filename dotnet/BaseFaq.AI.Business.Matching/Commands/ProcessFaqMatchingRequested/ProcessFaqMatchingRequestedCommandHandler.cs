using BaseFaq.AI.Business.Common.Utilities;
using BaseFaq.AI.Business.Matching.Abstractions;
using BaseFaq.Models.Ai.Contracts.Matching;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BaseFaq.AI.Business.Matching.Commands.ProcessFaqMatchingRequested;

public sealed class ProcessFaqMatchingRequestedCommandHandler(
    IMatchingExecutionService matchingExecutionService,
    ILogger<ProcessFaqMatchingRequestedCommandHandler> logger,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<ProcessFaqMatchingRequestedCommand>
{
    private const int MaxErrorMessageLength = 4000;
    private const string SimilarityErrorCode = "MATCHING_FAILED";

    public async Task Handle(ProcessFaqMatchingRequestedCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Message);

        var message = command.Message;

        try
        {
            var rankedCandidates = await matchingExecutionService.ExecuteAsync(message, cancellationToken);
            await PublishMatchingCompletedAsync(message, rankedCandidates, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Matching worker failed for CorrelationId {CorrelationId}, FaqItemId {FaqItemId}, TenantId {TenantId}.",
                message.CorrelationId,
                message.FaqItemId,
                message.TenantId);

            await PublishMatchingFailedSafeAsync(message, ex, cancellationToken);
        }
    }

    private async Task PublishMatchingCompletedAsync(
        FaqMatchingRequestedV1 message,
        MatchingCandidate[] rankedCandidates,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new FaqMatchingCompletedV1
        {
            EventId = Guid.NewGuid(),
            CorrelationId = message.CorrelationId,
            TenantId = message.TenantId,
            FaqItemId = message.FaqItemId,
            Candidates = rankedCandidates,
            OccurredUtc = DateTime.UtcNow
        }, cancellationToken);
    }

    private async Task PublishMatchingFailedSafeAsync(
        FaqMatchingRequestedV1 message,
        Exception ex,
        CancellationToken cancellationToken)
    {
        var errorMessage = TextBounds.Truncate(ex.Message, MaxErrorMessageLength);

        try
        {
            await publishEndpoint.Publish(new FaqMatchingFailedV1
            {
                EventId = Guid.NewGuid(),
                CorrelationId = message.CorrelationId,
                TenantId = message.TenantId,
                FaqItemId = message.FaqItemId,
                ErrorCode = SimilarityErrorCode,
                ErrorMessage = errorMessage,
                OccurredUtc = DateTime.UtcNow
            }, cancellationToken);
        }
        catch (Exception publishEx)
        {
            logger.LogError(
                publishEx,
                "Failed to publish matching failure callback. CorrelationId {CorrelationId}, FaqItemId {FaqItemId}, TenantId {TenantId}.",
                message.CorrelationId,
                message.FaqItemId,
                message.TenantId);
        }
    }
}