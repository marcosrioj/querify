using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Faq.Dtos.Faq;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaq;

public class FaqsGetFaqQueryHandler(FaqDbContext dbContext) : IRequestHandler<FaqsGetFaqQuery, FaqDto?>
{
    public async Task<FaqDto?> Handle(FaqsGetFaqQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Faqs
            .AsNoTracking()
            .Where(entity => entity.Id == request.Id)
            .Select(faq => new FaqDto
            {
                Id = faq.Id,
                Name = faq.Name,
                Language = faq.Language,
                Status = faq.Status,
                UpdatedDate = faq.UpdatedDate
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
