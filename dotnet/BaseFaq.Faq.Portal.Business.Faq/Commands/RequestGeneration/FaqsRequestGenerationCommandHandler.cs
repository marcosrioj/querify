using System.Net;
using BaseFaq.AI.Common.Contracts.Generation;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Faq.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.Faq.Portal.Business.Faq.Commands.RequestGeneration;

public sealed class FaqsRequestGenerationCommandHandler(
    FaqDbContext dbContext,
    ISessionService sessionService,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration) : IRequestHandler<FaqsRequestGenerationCommand, Guid>
{
    private static readonly ContentRefKind[] ProcessableContentRefKinds =
    [
        ContentRefKind.Web,
        ContentRefKind.Pdf,
        ContentRefKind.Document,
        ContentRefKind.Video
    ];

    private const int MaxLanguageLength = 16;
    private const int MaxPromptProfileLength = 128;
    private const string DefaultPromptProfile = "default";

    public async Task<Guid> Handle(
        FaqsRequestGenerationCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = CreateRequestContext(request);
        var faq = await LoadFaqOrThrowAsync(context.FaqId, context.TenantId, cancellationToken);
        ValidateFaqLanguage(faq.Language);
        await EnsureProcessableContentRefsAsync(context.FaqId, context.TenantId, cancellationToken);
        var promptProfile = GetPromptProfileOrThrow();
        await PublishGenerationRequestedAsync(context, faq.Language, promptProfile, cancellationToken);
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
        string promptProfile,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(new FaqGenerationRequestedV1
        {
            CorrelationId = context.CorrelationId,
            FaqId = context.FaqId,
            TenantId = context.TenantId,
            RequestedByUserId = context.UserId,
            Language = language,
            PromptProfile = promptProfile,
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

    private string GetPromptProfileOrThrow()
    {
        var promptProfile = (configuration["Ai:Generation:PromptProfile"] ?? DefaultPromptProfile).Trim();
        if (string.IsNullOrWhiteSpace(promptProfile) || promptProfile.Length > MaxPromptProfileLength)
        {
            throw new ApiErrorException(
                $"PromptProfile must be between 1 and {MaxPromptProfileLength} characters.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        return promptProfile;
    }

    private readonly record struct GenerationRequestContext(
        Guid FaqId,
        Guid TenantId,
        Guid UserId,
        Guid CorrelationId,
        DateTime RequestedUtc);
}