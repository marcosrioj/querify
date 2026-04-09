using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Projections;
using BaseFaq.Models.Faq.Dtos.FaqItem;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.FaqItem.Queries.GetFaqItem;

public class FaqItemsGetFaqItemQueryHandler(FaqDbContext dbContext)
    : IRequestHandler<FaqItemsGetFaqItemQuery, FaqItemDto?>
{
    public async Task<FaqItemDto?> Handle(FaqItemsGetFaqItemQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.FaqItems
            .AsNoTracking()
            .Where(entity => entity.Id == request.Id)
            .SelectPortalFaqItemDtos()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
