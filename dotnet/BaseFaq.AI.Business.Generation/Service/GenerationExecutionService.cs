using BaseFaq.AI.Business.Common.Abstractions;
using BaseFaq.AI.Business.Common.Utilities;
using BaseFaq.AI.Business.Generation.Abstractions;
using BaseFaq.AI.Business.Generation.Models;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Models.Ai.Contracts.Generation;
using BaseFaq.Models.Faq.Enums;
using BaseFaq.Models.Tenant.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BaseFaq.AI.Business.Generation.Service;

public sealed class GenerationExecutionService(
    IAiProviderContextResolver aiProviderContextResolver,
    IFaqDbContextFactory faqDbContextFactory,
    ContentRefStudyService contentRefStudyService,
    IGenerationPromptBuilder generationPromptBuilder,
    IGenerationProviderClient generationProviderClient,
    ILogger<GenerationExecutionService> logger)
    : IGenerationExecutionService
{
    public async Task ExecuteAsync(FaqGenerationRequestedV1 message, CancellationToken cancellationToken)
    {
        var providerContext = await aiProviderContextResolver.ResolveAsync(
            message.TenantId,
            AiCommandType.Generation,
            cancellationToken);

        var studiedRefs = await LoadStudiedRefsAsync(message, cancellationToken);
        var promptData = generationPromptBuilder.BuildPromptData(message, studiedRefs, providerContext);

        var generatedDraft = await generationProviderClient.GenerateDraftAsync(
            providerContext,
            promptData,
            cancellationToken);

        await WriteGeneratedFaqItemAsync(message, generatedDraft, cancellationToken);
    }

    private async Task<ContentRefStudyResult> LoadStudiedRefsAsync(
        FaqGenerationRequestedV1 message,
        CancellationToken cancellationToken)
    {
        await using var faqDbContext = faqDbContextFactory.Create(message.TenantId);

        var contentRefs = await faqDbContext.FaqContentRefs
            .AsNoTracking()
            .Where(x => x.FaqId == message.FaqId && x.TenantId == message.TenantId)
            .Select(x => new ValueTuple<ContentRefKind, string>(
                x.ContentRef.Kind,
                x.ContentRef.Locator ?? string.Empty))
            .ToListAsync(cancellationToken);

        if (contentRefs.Count == 0)
        {
            throw new InvalidOperationException(
                $"FAQ '{message.FaqId}' must have at least one ContentRef to continue generation.");
        }

        return contentRefStudyService.Study(contentRefs);
    }

    private async Task WriteGeneratedFaqItemAsync(
        FaqGenerationRequestedV1 message,
        GeneratedFaqDraft generatedDraft,
        CancellationToken cancellationToken)
    {
        await using var faqDbContext = faqDbContextFactory.Create(message.TenantId);

        var faq = await faqDbContext.Faqs
            .FirstOrDefaultAsync(x => x.Id == message.FaqId, cancellationToken);

        if (faq is null)
        {
            throw new InvalidOperationException(
                $"FAQ '{message.FaqId}' was not found for tenant '{message.TenantId}'.");
        }

        var itemId = DeterministicGuid.CreateV3(
            message.CorrelationId.ToString("N"),
            message.FaqId.ToString("N"),
            message.TenantId.ToString("N"));

        var question = TextBounds.Truncate(generatedDraft.Question, FaqItem.MaxQuestionLength);
        var shortAnswer = TextBounds.Truncate(generatedDraft.Summary, FaqItem.MaxShortAnswerLength);
        var answer = TextBounds.Truncate(generatedDraft.Answer, FaqItem.MaxAnswerLength);

        var faqItem = await faqDbContext.FaqItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.Id == itemId && x.FaqId == message.FaqId && x.TenantId == message.TenantId,
                cancellationToken);

        if (faqItem is null)
        {
            faqItem = new FaqItem
            {
                Id = itemId,
                TenantId = message.TenantId,
                FaqId = message.FaqId,
                Question = question,
                ShortAnswer = shortAnswer,
                Answer = answer,
                AdditionalInfo = null,
                CtaTitle = null,
                CtaUrl = null,
                VoteScore = 0,
                AiConfidenceScore = generatedDraft.Confidence,
                IsActive = true,
                Sort = await GetNextSortAsync(faqDbContext, message, cancellationToken)
            };

            faqDbContext.FaqItems.Add(faqItem);
        }
        else
        {
            faqItem.Question = question;
            faqItem.ShortAnswer = shortAnswer;
            faqItem.Answer = answer;
            faqItem.AdditionalInfo = null;
            faqItem.CtaTitle = null;
            faqItem.CtaUrl = null;
            faqItem.AiConfidenceScore = generatedDraft.Confidence;
            faqItem.IsActive = true;
            faqItem.IsDeleted = false;
            faqItem.DeletedDate = null;
            faqItem.DeletedBy = null;
        }

        await faqDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "FAQ generation write completed for CorrelationId {CorrelationId}, FaqId {FaqId}, TenantId {TenantId}.",
            message.CorrelationId,
            message.FaqId,
            message.TenantId);
    }

    private static async Task<int> GetNextSortAsync(
        FaqDbContext faqDbContext,
        FaqGenerationRequestedV1 message,
        CancellationToken cancellationToken)
    {
        var maxSort = await faqDbContext.FaqItems
            .Where(x => x.FaqId == message.FaqId && !x.IsDeleted)
            .Select(x => (int?)x.Sort)
            .MaxAsync(cancellationToken);

        return (maxSort ?? 0) + 1;
    }
}