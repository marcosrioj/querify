using BaseFaq.AI.Common.Contracts.Matching;
using BaseFaq.AI.Common.Persistence.AiDb;
using BaseFaq.AI.Common.Persistence.AiDb.Entities;
using BaseFaq.AI.Common.Providers.Abstractions;
using BaseFaq.AI.Matching.Business.Worker.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Ai.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BaseFaq.AI.Matching.Business.Worker.Commands.ProcessFaqMatchingRequested;

public sealed class ProcessFaqMatchingRequestedCommandHandler(
    AiDbContext aiDbContext,
    IAiProviderCredentialAccessor aiProviderCredentialAccessor,
    IMatchingFaqDbContextFactory matchingFaqDbContextFactory,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<ProcessFaqMatchingRequestedCommand>
{
    private const int MaxCandidates = 5;
    private const string SimilarityErrorCode = "MATCHING_FAILED";
    private static readonly Regex WordSplitter = new("[^a-z0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public async Task Handle(ProcessFaqMatchingRequestedCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Message);

        var message = command.Message;

        if (await IsMessageAlreadyProcessedAsync(command.HandlerName, command.MessageId, cancellationToken))
        {
            return;
        }

        try
        {
            var rankedCandidates = await BuildRankedCandidatesAsync(message, cancellationToken);
            await PublishMatchingCompletedAsync(message, rankedCandidates, cancellationToken);
            await MarkProcessedAsync(command.HandlerName, command.MessageId, cancellationToken);
        }
        catch (Exception ex)
        {
            await PublishMatchingFailedAsync(message, ex, cancellationToken);
            await MarkProcessedAsync(command.HandlerName, command.MessageId, cancellationToken);
        }
    }

    private async Task<bool> IsMessageAlreadyProcessedAsync(
        string handlerName,
        string messageId,
        CancellationToken cancellationToken)
    {
        return await aiDbContext.ProcessedMessages
            .AnyAsync(
                x => x.HandlerName == handlerName && x.MessageId == messageId,
                cancellationToken);
    }

    private async Task<MatchingCandidate[]> BuildRankedCandidatesAsync(
        FaqMatchingRequestedV1 message,
        CancellationToken cancellationToken)
    {
        _ = aiProviderCredentialAccessor.GetCurrent();
        await using var faqDbContext = matchingFaqDbContextFactory.Create(message.TenantId);

        var sourceQuestion = await LoadSourceQuestionAsync(faqDbContext, message, cancellationToken);
        var queryText = string.IsNullOrWhiteSpace(message.Query) ? sourceQuestion : message.Query;
        var candidates = await LoadCandidateQuestionsAsync(faqDbContext, message, cancellationToken);

        return candidates
            .Select(x => new MatchingCandidate(x.Id, ComputeSimilarity(queryText, x.Question)))
            .Where(x => x.SimilarityScore > 0)
            .OrderByDescending(x => x.SimilarityScore)
            .Take(MaxCandidates)
            .ToArray();
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

    private static async Task<List<CandidateQuestion>> LoadCandidateQuestionsAsync(
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

    private async Task PublishMatchingFailedAsync(
        FaqMatchingRequestedV1 message,
        Exception ex,
        CancellationToken cancellationToken)
    {
        var errorMessage = ex.Message.Length <= GenerationJob.MaxErrorMessageLength
            ? ex.Message
            : ex.Message[..GenerationJob.MaxErrorMessageLength];

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

    private async Task MarkProcessedAsync(string handlerName, string messageId, CancellationToken cancellationToken)
    {
        aiDbContext.ProcessedMessages.Add(new ProcessedMessage
        {
            HandlerName = handlerName,
            MessageId = messageId,
            ProcessedUtc = DateTime.UtcNow
        });

        try
        {
            await aiDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Duplicate dedupe key can happen with concurrent delivery handling.
        }
    }

    private static double ComputeSimilarity(string left, string right)
    {
        var leftTerms = Tokenize(left);
        var rightTerms = Tokenize(right);

        if (leftTerms.Count == 0 || rightTerms.Count == 0)
        {
            return 0;
        }

        var intersection = leftTerms.Intersect(rightTerms).Count();
        if (intersection == 0)
        {
            return 0;
        }

        var union = leftTerms.Union(rightTerms).Count();
        return Math.Round(intersection / (double)union, 4);
    }

    private static HashSet<string> Tokenize(string text)
    {
        return WordSplitter
            .Split(text.Trim().ToLowerInvariant())
            .Where(x => x.Length > 1)
            .ToHashSet(StringComparer.Ordinal);
    }

    private sealed record CandidateQuestion(Guid Id, string Question);
}