using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Answer.Commands.DeleteAnswer;

public sealed class AnswersDeleteAnswerCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersDeleteAnswerCommand>
{
    public async Task Handle(AnswersDeleteAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Answers
            .Include(answer => answer.FollowUpQuestions)
            .SingleOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        foreach (var followUpQuestion in entity.FollowUpQuestions.ToList())
        {
            followUpQuestion.ParentAnswerId = null;
            followUpQuestion.ParentAnswer = null;
            entity.FollowUpQuestions.Remove(followUpQuestion);
        }

        dbContext.Answers.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
