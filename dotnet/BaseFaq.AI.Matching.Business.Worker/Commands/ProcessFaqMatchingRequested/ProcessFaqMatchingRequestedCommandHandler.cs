using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using BaseFaq.AI.Common.Contracts.Matching;
using BaseFaq.AI.Common.Persistence.AiDb;
using BaseFaq.AI.Common.Persistence.AiDb.Entities;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.AI.Matching.Business.Worker.Commands.ProcessFaqMatchingRequested;

public sealed class ProcessFaqMatchingRequestedCommandHandler(
    AiDbContext aiDbContext,
    TenantDbContext tenantDbContext,
    ITenantConnectionStringProvider tenantConnectionStringProvider,
    IConfiguration configuration,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<ProcessFaqMatchingRequestedCommand>
{
    private const int MaxCandidates = 5;
    private const int MaxPromptCandidates = 100;
    private const string SimilarityErrorCode = "MATCHING_FAILED";
    private const string PromptDomain = "matching";
    private const string PromptVersion = "2026-02-15.matching.v1";
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
        var providerContext = await ResolveTenantAiProviderAsync(
            message.TenantId,
            AiCommandType.Matching,
            cancellationToken);

        await using var faqDbContext = CreateFaqDbContext(message.TenantId);

        var sourceQuestion = await LoadSourceQuestionAsync(faqDbContext, message, cancellationToken);
        var queryText = string.IsNullOrWhiteSpace(message.Query) ? sourceQuestion : message.Query;
        var candidates = await LoadCandidateQuestionsAsync(faqDbContext, message, cancellationToken);

        var promptData = BuildPromptData(
            sourceQuestion,
            queryText,
            message.Language,
            candidates.Select(x => new MatchingPromptCandidate(x.Id, x.Question)).ToArray(),
            providerContext);

        AddPromptActivityTags(promptData, candidates.Count, providerContext);

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

    private static void AddPromptActivityTags(
        MatchingPromptData promptData,
        int candidatesCount,
        MatchingAiProviderContext providerContext)
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
        IReadOnlyCollection<MatchingPromptCandidate> candidates,
        MatchingAiProviderContext providerContext)
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
        IReadOnlyCollection<MatchingPromptCandidate> candidates)
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
            builder.AppendLine($"- faqItemId={candidate.FaqItemId:D}, question={candidate.Question}");
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

    private async Task<MatchingAiProviderContext> ResolveTenantAiProviderAsync(
        Guid tenantId,
        AiCommandType commandType,
        CancellationToken cancellationToken)
    {
        var provider = await tenantDbContext.TenantAiProviders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AiProvider.Command == commandType)
            .OrderByDescending(x => x.AiProvider.Provider.ToLower() == "openai")
            .ThenBy(x => x.AiProvider.Provider)
            .ThenBy(x => x.AiProvider.Model)
            .Select(x => new MatchingAiProviderContext(
                x.AiProvider.Provider,
                x.AiProvider.Model,
                x.AiProvider.Prompt,
                x.AiProviderKey))
            .FirstOrDefaultAsync(cancellationToken);

        if (provider is null)
        {
            throw new InvalidOperationException(
                $"Tenant '{tenantId}' has no AI provider configured for '{commandType}'.");
        }

        return provider;
    }

    private FaqDbContext CreateFaqDbContext(Guid tenantId)
    {
        var connectionString = tenantConnectionStringProvider.GetConnectionString(tenantId);

        var options = new DbContextOptionsBuilder<FaqDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new FaqDbContext(
            options,
            new IntegrationSessionService(tenantId, ResolveAiUserId(configuration)),
            configuration,
            new StaticTenantConnectionStringProvider(connectionString),
            new HttpContextAccessor());
    }

    private static Guid ResolveAiUserId(IConfiguration configuration)
    {
        return Guid.TryParse(configuration["Ai:UserId"], out var configuredUserId)
            ? configuredUserId
            : Guid.Empty;
    }

    private sealed class IntegrationSessionService(Guid tenantId, Guid userId) : ISessionService
    {
        public Guid GetTenantId(AppEnum app) => tenantId;
        public Guid GetUserId() => userId;
    }

    private sealed class StaticTenantConnectionStringProvider(string connectionString)
        : ITenantConnectionStringProvider
    {
        public string GetConnectionString(Guid tenantId) => connectionString;
    }

    private sealed record MatchingAiProviderContext(
        string Provider,
        string Model,
        string? Prompt,
        string? ApiKey);

    private sealed record MatchingPromptCandidate(Guid FaqItemId, string Question);

    private sealed record MatchingPromptData(
        string Domain,
        string Version,
        string Provider,
        string Template,
        string Input,
        string OutputSchema);

    private sealed record CandidateQuestion(Guid Id, string Question);
}