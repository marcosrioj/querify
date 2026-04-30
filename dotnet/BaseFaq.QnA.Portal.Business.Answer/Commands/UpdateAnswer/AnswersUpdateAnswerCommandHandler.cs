using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Domain.BusinessRules.Activities;
using BaseFaq.QnA.Common.Domain.BusinessRules.Answers;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.UpdateAnswer;

public sealed class AnswersUpdateAnswerCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<AnswersUpdateAnswerCommand, Guid>
{
    public async Task<Guid> Handle(AnswersUpdateAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var actor = ActivityActorResolver.ResolvePortalActor(
            sessionService,
            httpContextAccessor,
            ActorKind.Moderator);
        var userId = actor.AuditUserId;
        var entity = await dbContext.Answers
            .Include(answer => answer.Question)
            .ThenInclude(question => question.Activities)
            .Include(answer => answer.Sources)
            .ThenInclude(link => link.Source)
            .SingleOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        var beforeSnapshot = ActivityEntityMetadata.SnapshotAnswer(entity);
        var originalStatus = entity.Status;
        Apply(entity, request.Request, userId);

        var afterSnapshot = ActivityEntityMetadata.SnapshotAnswer(entity);
        var statusChanged = entity.Status != originalStatus;
        ActivityAppender.AddAnswerActivity(
            entity,
            statusChanged
                ? ActivityKindStatusMap.ForAnswerStatus(entity.Status)
                : ActivityKind.AnswerUpdated,
            actor,
            statusChanged ? "StatusChanged" : "Updated",
            beforeSnapshot,
            afterSnapshot,
            ActivityEntityMetadata.AnswerContext(entity));

        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    private static void Apply(Common.Domain.Entities.Answer entity, AnswerUpdateRequestDto request, string userId)
    {
        entity.Headline = request.Headline;
        entity.Body = request.Body;
        entity.AuthorLabel = request.AuthorLabel;
        entity.ContextNote = request.ContextNote;
        entity.Sort = request.Sort;
        entity.Kind = request.Kind;

        AnswerRules.SetSupportedStatus(entity, request.Status);

        AnswerRules.EnsureVisibilityAllowed(entity, request.Visibility);
        entity.Visibility = request.Visibility;
        entity.UpdatedBy = userId;
    }
}
