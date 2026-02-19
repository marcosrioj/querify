using BaseFaq.AI.Business.Common.Abstractions;
using BaseFaq.AI.Business.Matching.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Ai.Contracts.Matching;
using BaseFaq.Models.Tenant.Enums;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.AI.Business.Matching.Service;

public sealed class MatchingExecutionService(
    IAiProviderContextResolver aiProviderContextResolver,
    IFaqDbContextFactory faqDbContextFactory,
    IMatchingProviderClient matchingProviderClient)
    : IMatchingExecutionService
{
    private const int MaxCandidates = 5;

    public async Task<MatchingCandidate[]> ExecuteAsync(
        FaqMatchingRequestedV1 message,
        CancellationToken cancellationToken)
    {
        var providerContext = await aiProviderContextResolver.ResolveAsync(
            message.TenantId,
            AiCommandType.Matching,
            cancellationToken);

        await using var faqDbContext = faqDbContextFactory.Create(message.TenantId);

        var sourceQuestion = await LoadSourceQuestionAsync(faqDbContext, message, cancellationToken);
        var queryText = string.IsNullOrWhiteSpace(message.Query) ? sourceQuestion : message.Query;
        var candidates = await LoadCandidateQuestionsAsync(faqDbContext, message, cancellationToken);

        return await matchingProviderClient.RankAsync(
            providerContext,
            queryText,
            candidates,
            MaxCandidates,
            cancellationToken);
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

    private static async Task<IReadOnlyList<(Guid Id, string Question)>> LoadCandidateQuestionsAsync(
        FaqDbContext faqDbContext,
        FaqMatchingRequestedV1 message,
        CancellationToken cancellationToken)
    {
        return await faqDbContext.FaqItems
            .AsNoTracking()
            .Where(x => x.TenantId == message.TenantId && x.Id != message.FaqItemId && x.IsActive)
            .Select(x => new ValueTuple<Guid, string>(x.Id, x.Question ?? string.Empty))
            .ToListAsync(cancellationToken);
    }
}