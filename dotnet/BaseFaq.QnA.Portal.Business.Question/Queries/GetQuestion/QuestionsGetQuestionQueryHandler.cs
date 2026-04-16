using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestion;

public sealed class QuestionsGetQuestionQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsGetQuestionQuery, QuestionDetailDto>
{
    public async Task<QuestionDetailDto> Handle(QuestionsGetQuestionQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await dbContext.Questions
            .Include(question => question.Space)
            .Include(question => question.AcceptedAnswer)
            .ThenInclude(answer => answer!.Sources)
            .ThenInclude(link => link.Source)
            .Include(question => question.Answers)
            .ThenInclude(answer => answer.Sources)
            .ThenInclude(link => link.Source)
            .Include(question => question.Sources)
            .ThenInclude(link => link.Source)
            .Include(question => question.QuestionTopics)
            .ThenInclude(link => link.Topic)
            .Include(question => question.Activity)
            .AsNoTracking()
            .SingleOrDefaultAsync(question => question.TenantId == tenantId && question.Id == request.Id,
                cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Question '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        return entity.ToPortalQuestionDetailDto();
    }
}