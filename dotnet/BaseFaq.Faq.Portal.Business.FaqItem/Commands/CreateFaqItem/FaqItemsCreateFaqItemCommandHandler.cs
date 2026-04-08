using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Common.Enums;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.FaqItem.Commands.CreateFaqItem;

public class FaqItemsCreateFaqItemCommandHandler(FaqDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<FaqItemsCreateFaqItemCommand, Guid>
{
    public async Task<Guid> Handle(FaqItemsCreateFaqItemCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.Faq);

        var faqItem = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = request.Question,
            ShortAnswer = request.ShortAnswer,
            Answer = request.Answer,
            AdditionalInfo = request.AdditionalInfo,
            CtaTitle = request.CtaTitle,
            CtaUrl = request.CtaUrl,
            Sort = request.Sort,
            IsActive = request.IsActive,
            FaqId = request.FaqId,
            ContentRefId = request.ContentRefId,
            TenantId = tenantId
        };

        await dbContext.FaqItems.AddAsync(faqItem, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return faqItem.Id;
    }
}