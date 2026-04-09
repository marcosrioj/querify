using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Common.Enums;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.CreateFaqItemAnswer;

public class FaqItemAnswersCreateFaqItemAnswerCommandHandler(FaqDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<FaqItemAnswersCreateFaqItemAnswerCommand, Guid>
{
    public async Task<Guid> Handle(FaqItemAnswersCreateFaqItemAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.Faq);

        var faqItemAnswer = new Common.Persistence.FaqDb.Entities.FaqItemAnswer
        {
            ShortAnswer = request.ShortAnswer,
            Answer = request.Answer,
            Sort = request.Sort,
            VoteScore = 0,
            IsActive = request.IsActive,
            FaqItemId = request.FaqItemId,
            TenantId = tenantId
        };

        await dbContext.FaqItemAnswers.AddAsync(faqItemAnswer, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return faqItemAnswer.Id;
    }
}
