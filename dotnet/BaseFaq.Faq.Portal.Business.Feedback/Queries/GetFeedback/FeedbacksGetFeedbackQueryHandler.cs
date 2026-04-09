using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Faq.Dtos.Feedback;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.Feedback.Queries.GetFeedback;

public class FeedbacksGetFeedbackQueryHandler(FaqDbContext dbContext) : IRequestHandler<FeedbacksGetFeedbackQuery, FeedbackDto?>
{
    public async Task<FeedbackDto?> Handle(FeedbacksGetFeedbackQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Feedbacks
            .AsNoTracking()
            .Where(entity => entity.Id == request.Id)
            .Select(feedback => new FeedbackDto
            {
                Id = feedback.Id,
                Like = feedback.Like,
                UserPrint = feedback.UserPrint,
                Ip = feedback.Ip,
                UserAgent = feedback.UserAgent,
                UnLikeReason = feedback.UnLikeReason,
                FaqItemId = feedback.FaqItemId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}