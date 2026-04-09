using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Faq.Dtos.FaqItemAnswer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.FaqItemAnswer.Queries.GetFaqItemAnswer;

public class FaqItemAnswersGetFaqItemAnswerQueryHandler(FaqDbContext dbContext)
    : IRequestHandler<FaqItemAnswersGetFaqItemAnswerQuery, FaqItemAnswerDto?>
{
    public async Task<FaqItemAnswerDto?> Handle(
        FaqItemAnswersGetFaqItemAnswerQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.FaqItemAnswers
            .AsNoTracking()
            .Where(entity => entity.Id == request.Id)
            .Select(answer => new FaqItemAnswerDto
            {
                Id = answer.Id,
                ShortAnswer = answer.ShortAnswer,
                Answer = answer.Answer,
                Sort = answer.Sort,
                VoteScore = answer.VoteScore,
                IsActive = answer.IsActive,
                FaqItemId = answer.FaqItemId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
