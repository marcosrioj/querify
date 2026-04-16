using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ThreadActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.ThreadActivity;

namespace BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswer;

public sealed class AnswersGetAnswerQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersGetAnswerQuery, AnswerDto>
{
    public async Task<AnswerDto> Handle(AnswersGetAnswerQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await dbContext.Answers
            .Include(answer => answer.Question)
            .ThenInclude(question => question.Activities)
            .Include(answer => answer.Sources)
            .ThenInclude(link => link.Source)
            .AsNoTracking()
            .SingleOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        IEnumerable<ThreadActivityEntity> questionActivity = entity.Question?.Activities ?? [];
        return entity.ToPortalAnswerDto(questionActivity, entity.Question?.AcceptedAnswerId);
    }
}
