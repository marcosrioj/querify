using BaseFaq.AI.Business.Common.Abstractions;
using BaseFaq.AI.Business.Common.Utilities;
using BaseFaq.AI.Business.Matching.Abstractions;
using BaseFaq.AI.Business.Matching.Dtos;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Ai.Contracts.Matching;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BaseFaq.AI.Business.Matching.Commands.ProcessFaqMatchingRequested;

public sealed class ProcessFaqMatchingRequestedCommandHandler(
    IFaqDbContextFactory faqDbContextFactory,
    IFaqMatchingScorer faqMatchingScorer,
    ILogger<ProcessFaqMatchingRequestedCommandHandler> logger,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<ProcessFaqMatchingRequestedCommand>
{
    private const int MaxCandidates = 5;
    private const int MaxErrorMessageLength = 2000;
    private const string SimilarityErrorCode = "MATCHING_FAILED";

    public async Task Handle(ProcessFaqMatchingRequestedCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Message);

        var message = command.Message;

        try
        {
            var rankedCandidates = await BuildRankedCandidatesAsync(message, cancellationToken);
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

    private async Task<MatchingCandidate[]> BuildRankedCandidatesAsync(
        FaqMatchingRequestedV1 message,
        CancellationToken cancellationToken)
    {
        await using var faqDbContext = faqDbContextFactory.Create(message.TenantId);

        var sourceQuestion = await LoadSourceQuestionAsync(faqDbContext, message, cancellationToken);
        var queryText = string.IsNullOrWhiteSpace(message.Query) ? sourceQuestion : message.Query;
        var candidates = await LoadCandidateQuestionsAsync(faqDbContext, message, cancellationToken);

        return faqMatchingScorer.Rank(queryText, candidates, MaxCandidates);
    }

    private static async Task<string> LoadSourceQuestionAsync(
        FaqDbContext faqDbContext,
        FaqMatchingRequestedV1 message,
        CancellationToken cancellationToken)
    {
        var sourceQuestion = await faqDbContext.FaqItems
            .AsNoTracking()
            .Where(x => x.Id == message.FaqItemId && x.TenantId == message.TenantId)
            .Select(x => x.Question)
            .SingleOrDefaultAsync(cancellationToken);

        if (sourceQuestion is null)
        {
            throw new ArgumentException("FaqItemId does not exist for the tenant.", nameof(message.FaqItemId));
        }

        return sourceQuestion;
    }

    private static async Task<IReadOnlyList<CandidateQuestion>> LoadCandidateQuestionsAsync(
        FaqDbContext faqDbContext,
        FaqMatchingRequestedV1 message,
        CancellationToken cancellationToken)
    {
        return await faqDbContext.FaqItems
            .AsNoTracking()
            .Where(x => x.TenantId == message.TenantId && x.Id != message.FaqItemId && x.IsActive)
            .Select(x => new CandidateQuestion(x.Id, x.Question))
            .ToListAsync(cancellationToken);
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