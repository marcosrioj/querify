using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.Faq.Portal.Business.Faq.Commands.UpdateFaqTag;

public class FaqTagsUpdateFaqTagCommandHandler(FaqDbContext dbContext)
    : IRequestHandler<FaqTagsUpdateFaqTagCommand>
{
    public async Task Handle(FaqTagsUpdateFaqTagCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var faqTag = await GetFaqTagOrThrowAsync(request.Id, cancellationToken);
        await EnsureFaqExistsAsync(request.FaqId, cancellationToken);
        await EnsureTagExistsAsync(request.TagId, cancellationToken);
        faqTag.FaqId = request.FaqId;
        faqTag.TagId = request.TagId;

        dbContext.FaqTags.Update(faqTag);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Common.Persistence.FaqDb.Entities.FaqTag> GetFaqTagOrThrowAsync(
        Guid faqTagId,
        CancellationToken cancellationToken)
    {
        var faqTag = await dbContext.FaqTags.FirstOrDefaultAsync(
            entity => entity.Id == faqTagId,
            cancellationToken);

        if (faqTag is null)
        {
            throw new ApiErrorException(
                $"FAQ tag '{faqTagId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return faqTag;
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
}