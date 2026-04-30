using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Domain.BusinessRules.Activities;
using BaseFaq.QnA.Common.Domain.BusinessRules.Answers;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.ActivateAnswer;

public sealed class AnswersActivateAnswerCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<AnswersActivateAnswerCommand, Guid>
{
    public async Task<Guid> Handle(AnswersActivateAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var actor = ActivityActorResolver.ResolvePortalActor(
            sessionService,
            httpContextAccessor,
            ActorKind.Moderator);
        var entity = await dbContext.Answers
            .Include(answer => answer.Question)
            .ThenInclude(question => question.Activities)
            .SingleOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        var originalStatus = entity.Status;
        AnswerRules.Activate(entity);

        if (originalStatus != entity.Status)
            ActivityAppender.AddAnswerActivity(
                entity,
                ActivityKindStatusMap.ForAnswerStatus(entity.Status),
                actor,
                "StatusChanged",
                new Dictionary<string, object?>(StringComparer.Ordinal) { ["Status"] = originalStatus.ToString() },
                new Dictionary<string, object?>(StringComparer.Ordinal) { ["Status"] = entity.Status.ToString() },
                ActivityEntityMetadata.AnswerContext(entity));

        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
