using System.Diagnostics;
using BaseFaq.AI.Business.Common.Abstractions;
using BaseFaq.AI.Business.Common.Utilities;
using BaseFaq.AI.Business.Generation.Observability;
using BaseFaq.AI.Business.Generation.Service;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Models.Ai.Contracts.Generation;
using BaseFaq.Models.Tenant.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BaseFaq.AI.Business.Generation.Commands.ProcessFaqGenerationRequested;

public sealed class ProcessFaqGenerationRequestedCommandHandler(
    ITenantAiProviderContextResolver tenantAiProviderContextResolver,
    IFaqDbContextFactory faqDbContextFactory,
    IFaqGenerationEngine generationEngine,
    ILogger<ProcessFaqGenerationRequestedCommandHandler> logger,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<ProcessFaqGenerationRequestedCommand>
{
    private const string GenerationErrorCode = "GENERATION_FAILED";
    private const int MaxErrorMessageLength = 2000;

    public async Task Handle(ProcessFaqGenerationRequestedCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Message);

        var message = command.Message;

        try
        {
            await ProcessGenerationAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Generation worker failed for CorrelationId {CorrelationId}, FaqId {FaqId}, TenantId {TenantId}.",
                message.CorrelationId,
                message.FaqId,
                message.TenantId);

            await PublishGenerationFailedSafeAsync(message, Guid.NewGuid(), ex, cancellationToken);
        }
    }

    private async Task ProcessGenerationAsync(
        FaqGenerationRequestedV1 message,
        CancellationToken cancellationToken)
    {
        var providerContext = await tenantAiProviderContextResolver.ResolveAsync(
            message.TenantId,
            AiCommandType.Generation,
            cancellationToken);

        var studiedRefs = await LoadStudiedRefsAsync(message, cancellationToken);
        var generatedDraft = generationEngine.Generate(message, studiedRefs, providerContext);

        using var providerActivity =
            GenerationWorkerTracing.ActivitySource.StartActivity("generation.provider.generate", ActivityKind.Client);

        AddProviderActivityTags(providerActivity, providerContext, message, studiedRefs, generatedDraft.PromptData);

        await WriteGeneratedFaqItemAsync(message, generatedDraft, cancellationToken);
        await PublishGenerationReadyAsync(message, Guid.NewGuid(), cancellationToken);
    }

    private async Task<ContentRefStudyResult> LoadStudiedRefsAsync(
        FaqGenerationRequestedV1 message,
        CancellationToken cancellationToken)
    {
        await using var faqDbContext = faqDbContextFactory.Create(message.TenantId);

        var contentRefs = await faqDbContext.FaqContentRefs
            .AsNoTracking()
            .Where(x => x.FaqId == message.FaqId && x.TenantId == message.TenantId)
            .Select(x => new ContentRefStudyInput(x.ContentRef.Kind, x.ContentRef.Locator))
            .ToListAsync(cancellationToken);

        if (contentRefs.Count == 0)
        {
            throw new InvalidOperationException(
                $"FAQ '{message.FaqId}' must have at least one ContentRef to continue generation.");
        }

        return ContentRefStudyService.Study(contentRefs);
    }

    private async Task WriteGeneratedFaqItemAsync(
        FaqGenerationRequestedV1 message,
        GeneratedFaqDraft generatedDraft,
        CancellationToken cancellationToken)
    {
        using var writeActivity =
            GenerationWorkerTracing.ActivitySource.StartActivity("generation.faq_db.write", ActivityKind.Internal);

        writeActivity?.SetTag("db.system", "postgresql");
        writeActivity?.SetTag("db.name", "bf_faq_db");
        writeActivity?.SetTag("basefaq.correlation_id", message.CorrelationId.ToString("D"));
        writeActivity?.SetTag("basefaq.tenant_id", message.TenantId.ToString("D"));
        writeActivity?.SetTag("basefaq.faq_id", message.FaqId.ToString("D"));

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

    private static void AddProviderActivityTags(
        Activity? providerActivity,
        BaseFaq.AI.Business.Common.Models.AiProviderContext providerContext,
        FaqGenerationRequestedV1 message,
        ContentRefStudyResult studiedRefs,
        GenerationPromptData promptData)
    {
        providerActivity?.SetTag("gen_ai.system", providerContext.Provider);
        providerActivity?.SetTag("gen_ai.request.model", providerContext.Model);
        providerActivity?.SetTag("basefaq.ai_key_configured", !string.IsNullOrWhiteSpace(providerContext.ApiKey));
        providerActivity?.SetTag("basefaq.correlation_id", message.CorrelationId.ToString("D"));
        providerActivity?.SetTag("basefaq.content_ref.total_count", studiedRefs.TotalCount);
        providerActivity?.SetTag("basefaq.content_ref.processed_count", studiedRefs.ProcessedCount);
        providerActivity?.SetTag("basefaq.content_ref.skipped_count", studiedRefs.SkippedCount);
        providerActivity?.SetTag("basefaq.prompt.domain", promptData.Domain);
        providerActivity?.SetTag("basefaq.prompt.version", promptData.Version);
        providerActivity?.SetTag("basefaq.prompt.provider", promptData.Provider);
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
