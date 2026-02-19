using BaseFaq.AI.Business.Common.Abstractions;
using BaseFaq.AI.Business.Generation.Abstractions;
using BaseFaq.AI.Business.Generation.Models;
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

        logger.LogInformation(
            "FAQ generation completed without DB persistence for CorrelationId {CorrelationId}, FaqId {FaqId}, TenantId {TenantId}. DraftQuestionLength={DraftQuestionLength}, DraftAnswerLength={DraftAnswerLength}, Confidence={Confidence}.",
            message.CorrelationId,
            message.FaqId,
            message.TenantId,
            generatedDraft.Question.Length,
            generatedDraft.Answer.Length,
            generatedDraft.Confidence);
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
}