using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.Faq.Portal.Business.Faq.Commands.UpdateFaqContentRef;

public class FaqContentRefsUpdateFaqContentRefCommandHandler(FaqDbContext dbContext)
    : IRequestHandler<FaqContentRefsUpdateFaqContentRefCommand>
{
    public async Task Handle(FaqContentRefsUpdateFaqContentRefCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var faqContentRef = await GetFaqContentRefOrThrowAsync(request.Id, cancellationToken);
        await EnsureFaqExistsAsync(request.FaqId, cancellationToken);
        await EnsureContentRefExistsAsync(request.ContentRefId, cancellationToken);
        faqContentRef.FaqId = request.FaqId;
        faqContentRef.ContentRefId = request.ContentRefId;

        dbContext.FaqContentRefs.Update(faqContentRef);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Common.Persistence.FaqDb.Entities.FaqContentRef> GetFaqContentRefOrThrowAsync(
        Guid faqContentRefId,
        CancellationToken cancellationToken)
    {
        var faqContentRef = await dbContext.FaqContentRefs
            .FirstOrDefaultAsync(entity => entity.Id == faqContentRefId, cancellationToken);

        if (faqContentRef is null)
        {
            throw new ApiErrorException(
                $"FAQ content reference '{faqContentRefId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return faqContentRef;
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

    private async Task EnsureContentRefExistsAsync(Guid contentRefId, CancellationToken cancellationToken)
    {
        var contentRefExists = await dbContext.ContentRefs.AnyAsync(
            entity => entity.Id == contentRefId,
            cancellationToken);
        if (!contentRefExists)
        {
            throw new ApiErrorException(
                $"Content reference '{contentRefId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }
    }
}