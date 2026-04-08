using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.Faq.Portal.Business.Faq.Commands.UpdateFaq;

public class FaqsUpdateFaqCommandHandler(FaqDbContext dbContext)
    : IRequestHandler<FaqsUpdateFaqCommand>
{
    public async Task Handle(FaqsUpdateFaqCommand request, CancellationToken cancellationToken)
    {
        var faq = await dbContext.Faqs
            .FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (faq is null)
        {
            throw new ApiErrorException(
                $"FAQ '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        faq.Name = request.Name;
        faq.Language = request.Language;
        faq.Status = request.Status;

        dbContext.Faqs.Update(faq);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
