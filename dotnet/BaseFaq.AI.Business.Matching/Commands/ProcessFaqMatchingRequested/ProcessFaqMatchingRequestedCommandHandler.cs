using System.Diagnostics;
using System.Text;
using BaseFaq.AI.Business.Common.Abstractions;
using BaseFaq.AI.Business.Common.Models;
using BaseFaq.AI.Business.Common.Utilities;
using BaseFaq.AI.Business.Matching.Service;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Ai.Contracts.Matching;
using BaseFaq.Models.Tenant.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BaseFaq.AI.Business.Matching.Commands.ProcessFaqMatchingRequested;

public sealed class ProcessFaqMatchingRequestedCommandHandler(
    ITenantAiProviderContextResolver tenantAiProviderContextResolver,
    IFaqDbContextFactory faqDbContextFactory,
    IFaqMatchingScorer faqMatchingScorer,
    ILogger<ProcessFaqMatchingRequestedCommandHandler> logger,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<ProcessFaqMatchingRequestedCommand>
{
    private const int MaxCandidates = 5;
    private const int MaxPromptCandidates = 100;
    private const int MaxErrorMessageLength = 2000;
    private const string SimilarityErrorCode = "MATCHING_FAILED";
    private const string PromptDomain = "matching";
    private const string PromptVersion = "2026-02-19.matching.v2";

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
        var providerContext = await tenantAiProviderContextResolver.ResolveAsync(
            message.TenantId,
            AiCommandType.Matching,
            cancellationToken);

        await using var faqDbContext = faqDbContextFactory.Create(message.TenantId);

        var sourceQuestion = await LoadSourceQuestionAsync(faqDbContext, message, cancellationToken);
        var queryText = string.IsNullOrWhiteSpace(message.Query) ? sourceQuestion : message.Query;
        var candidates = await LoadCandidateQuestionsAsync(faqDbContext, message, cancellationToken);

        var promptData = BuildPromptData(
            sourceQuestion,
            queryText,
            message.Language,
            candidates,
            providerContext);

        AddPromptActivityTags(promptData, candidates.Count, providerContext);

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

    private static void AddPromptActivityTags(
        MatchingPromptData promptData,
        int candidatesCount,
        AiProviderContext providerContext)
    {
        Activity.Current?.SetTag("gen_ai.system", providerContext.Provider);
        Activity.Current?.SetTag("gen_ai.request.model", providerContext.Model);
        Activity.Current?.SetTag("basefaq.ai_key_configured", !string.IsNullOrWhiteSpace(providerContext.ApiKey));
        Activity.Current?.SetTag("basefaq.prompt.domain", promptData.Domain);
        Activity.Current?.SetTag("basefaq.prompt.version", promptData.Version);
        Activity.Current?.SetTag("basefaq.prompt.provider", promptData.Provider);
        Activity.Current?.SetTag("basefaq.matching.candidates_count", candidatesCount);
    }

    private static MatchingPromptData BuildPromptData(
        string sourceQuestion,
        string queryText,
        string language,
        IReadOnlyCollection<CandidateQuestion> candidates,
        AiProviderContext providerContext)
    {
        var provider = NormalizeProvider(providerContext.Provider);

        return new MatchingPromptData(
            PromptDomain,
            PromptVersion,
            provider,
            ResolvePromptTemplate(providerContext.Prompt),
            BuildPromptInput(sourceQuestion, queryText, language, candidates),
            BuildOutputSchema());
    }

    private static string ResolvePromptTemplate(string? prompt)
    {
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            return prompt;
        }

        return
            "You are a FAQ semantic matching engine. Rank candidates by semantic relevance and return deterministic JSON.";
    }

    private static string BuildPromptInput(
        string sourceQuestion,
        string queryText,
        string language,
        IReadOnlyCollection<CandidateQuestion> candidates)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Task: rank FAQ candidates by semantic similarity.");
        builder.AppendLine($"language: {language}");
        builder.AppendLine($"sourceQuestion: {sourceQuestion}");
        builder.AppendLine($"query: {queryText}");
        builder.AppendLine($"candidatesTotal: {candidates.Count}");
        builder.AppendLine("candidates:");

        foreach (var candidate in candidates.Take(MaxPromptCandidates))
        {
            builder.AppendLine($"- faqItemId={candidate.Id:D}, question={candidate.Question}");
        }

        builder.AppendLine("Return top 5 candidates with score in [0,1], sorted descending.");
        return builder.ToString();
    }

    private static string BuildOutputSchema()
    {
        return """
               {
                 "type": "object",
                 "required": ["topCandidates"],
                 "properties": {
                   "topCandidates": {
                     "type": "array",
                     "maxItems": 5,
                     "items": {
                       "type": "object",
                       "required": ["faqItemId", "score", "reason"],
                       "properties": {
                         "faqItemId": { "type": "string" },
                         "score": { "type": "number", "minimum": 0, "maximum": 1 },
                         "reason": { "type": "string", "maxLength": 300 }
                       }
                     }
                   }
                 }
               }
               """;
    }

    private static string NormalizeProvider(string? provider)
    {
        return string.IsNullOrWhiteSpace(provider)
            ? "unknown"
            : provider.Trim().ToLowerInvariant();
    }

    private sealed record MatchingPromptData(
        string Domain,
        string Version,
        string Provider,
        string Template,
        string Input,
        string OutputSchema);
}
