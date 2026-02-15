using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Models.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.Faq.Portal.Business.Faq.Commands.CreateFaqTag;

public class FaqTagsCreateFaqTagCommandHandler(FaqDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<FaqTagsCreateFaqTagCommand, Guid>
{
    public async Task<Guid> Handle(FaqTagsCreateFaqTagCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenantId = sessionService.GetTenantId(AppEnum.Faq);
        await EnsureFaqExistsAsync(request.FaqId, cancellationToken);
        await EnsureTagExistsAsync(request.TagId, cancellationToken);
        return await CreateFaqTagAsync(request, tenantId, cancellationToken);
    }

    private async Task EnsureFaqExistsAsync(Guid faqId, CancellationToken cancellationToken)
    {
        var faqExists = await dbContext.Faqs.AnyAsync(entity => entity.Id == faqId, cancellationToken);
        if (!faqExists)
        {
            throw new ApiErrorException(
                $"FAQ '{faqId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }
    }

    private async Task EnsureTagExistsAsync(Guid tagId, CancellationToken cancellationToken)
    {
        var tagExists = await dbContext.Tags.AnyAsync(entity => entity.Id == tagId, cancellationToken);
        if (!tagExists)
        {
            throw new ApiErrorException(
                $"Tag '{tagId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }
    }

    private async Task<Guid> CreateFaqTagAsync(
        FaqTagsCreateFaqTagCommand request,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var faqTag = new FaqTag
        {
            FaqId = request.FaqId,
            TagId = request.TagId,
            TenantId = tenantId
        };

        await dbContext.FaqTags.AddAsync(faqTag, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return faqTag.Id;
    }
}