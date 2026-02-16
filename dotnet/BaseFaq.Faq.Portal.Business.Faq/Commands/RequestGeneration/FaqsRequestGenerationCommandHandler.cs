using System.Net;
using BaseFaq.AI.Common.Contracts.Generation;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Faq.Enums;
using BaseFaq.Models.Tenant.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.Faq.Commands.RequestGeneration;

public sealed class FaqsRequestGenerationCommandHandler(
    FaqDbContext dbContext,
    ISessionService sessionService,
    ITenantAiProviderResolver tenantAiProviderResolver,
    IPublishEndpoint publishEndpoint) : IRequestHandler<FaqsRequestGenerationCommand, Guid>
{
    private static readonly ContentRefKind[] ProcessableContentRefKinds =
    [
        ContentRefKind.Web,
        ContentRefKind.Pdf,
        ContentRefKind.Document,
        ContentRefKind.Video
    ];

    private const int MaxLanguageLength = 16;

    public async Task<Guid> Handle(
        FaqsRequestGenerationCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = CreateRequestContext(request);
        var faq = await LoadFaqOrThrowAsync(context.FaqId, context.TenantId, cancellationToken);
        ValidateFaqLanguage(faq.Language);
        await EnsureProcessableContentRefsAsync(context.FaqId, context.TenantId, cancellationToken);
        await PublishGenerationRequestedAsync(context, faq.Language, cancellationToken);
        return context.CorrelationId;
    }

    private GenerationRequestContext CreateRequestContext(FaqsRequestGenerationCommand request)
    {
        return new GenerationRequestContext(
            request.FaqId,
            sessionService.GetTenantId(AppEnum.Faq),
            sessionService.GetUserId(),
            Guid.NewGuid(),
            DateTime.UtcNow);
    }

    private async Task PublishGenerationRequestedAsync(
        GenerationRequestContext context,
        string language,
        CancellationToken cancellationToken)
    {
        var shouldPublish = await tenantAiProviderResolver.HasProviderForCommandAsync(
            context.TenantId,
            AiCommandType.Generation,
            cancellationToken);
        if (!shouldPublish)
        {
            throw new ApiErrorException(
                $"Tenant '{context.TenantId}' has no AI provider configured for generation.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        await publishEndpoint.Publish(new FaqGenerationRequestedV1
        {
            CorrelationId = context.CorrelationId,
            FaqId = context.FaqId,
            TenantId = context.TenantId,
            RequestedByUserId = context.UserId,
            Language = language,
            IdempotencyKey = $"faq-generation-{context.FaqId:N}-{context.CorrelationId:N}",
            RequestedUtc = context.RequestedUtc
        }, cancellationToken);
    }

    private async Task<Common.Persistence.FaqDb.Entities.Faq> LoadFaqOrThrowAsync(
        Guid faqId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var faq = await dbContext.Faqs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == faqId && x.TenantId == tenantId, cancellationToken);

        if (faq is null)
        {
            throw new ApiErrorException(
                $"FAQ '{faqId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return faq;
    }

    private static void ValidateFaqLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language) || language.Length > MaxLanguageLength)
        {
            throw new ApiErrorException(
                $"FAQ language is required and must have at most {MaxLanguageLength} characters to request generation.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }
    }

    private async Task EnsureProcessableContentRefsAsync(
        Guid faqId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var contentRefs = await dbContext.FaqContentRefs
            .AsNoTracking()
            .Where(x => x.FaqId == faqId && x.TenantId == tenantId)
            .Select(x => x.ContentRef.Kind)
            .ToListAsync(cancellationToken);

        if (contentRefs.Count == 0)
        {
            throw new ApiErrorException(
                $"FAQ '{faqId}' must have at least one ContentRef to request generation.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        var hasProcessableContentRef = contentRefs.Any(kind => ProcessableContentRefKinds.Contains(kind));
        if (!hasProcessableContentRef)
        {
            throw new ApiErrorException(
                $"FAQ '{faqId}' has ContentRef entries, but none with a processable kind (Web, Pdf, Document, Video).",
                errorCode: (int)HttpStatusCode.BadRequest);
        }
    }

    private readonly record struct GenerationRequestContext(
        Guid FaqId,
        Guid TenantId,
        Guid UserId,
        Guid CorrelationId,
        DateTime RequestedUtc);
}