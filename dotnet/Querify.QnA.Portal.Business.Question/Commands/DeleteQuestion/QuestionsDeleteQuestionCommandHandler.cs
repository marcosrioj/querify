using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Question.Commands.DeleteQuestion;

public sealed class QuestionsDeleteQuestionCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsDeleteQuestionCommand>
{
    public async Task Handle(QuestionsDeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Questions
            .SingleOrDefaultAsync(question => question.TenantId == tenantId && question.Id == request.Id,
                cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Question '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        var answers = await dbContext.Answers
            .Where(answer => answer.TenantId == tenantId && answer.QuestionId == entity.Id)
            .ToListAsync(cancellationToken);

        dbContext.Answers.RemoveRange(answers);
        dbContext.Questions.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
