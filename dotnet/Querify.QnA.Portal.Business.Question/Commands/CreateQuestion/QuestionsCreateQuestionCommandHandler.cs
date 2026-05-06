using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Question;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Activities;
using Querify.QnA.Common.Domain.BusinessRules.Questions;
using Querify.QnA.Common.Domain.BusinessRules.Spaces;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Question.Commands.CreateQuestion;

public sealed class QuestionsCreateQuestionCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsCreateQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsCreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var actor = ActivityActorResolver.ResolvePortalActor(
            sessionService,
            httpContextAccessor,
            ActorKind.Moderator);
        var userId = actor.AuditUserId;
        var space = await dbContext.Spaces
            .Include(entity => entity.Questions)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.SpaceId,
                cancellationToken);

        if (space is null)
            throw new ApiErrorException($"Space '{request.Request.SpaceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        SpaceRules.EnsureAcceptsQuestions(space);

        QuestionRules.EnsureSupportedStatus(request.Request.Status);

        var entity = new Common.Domain.Entities.Question
        {
            TenantId = tenantId,
            SpaceId = space.Id,
            Space = space,
            Title = request.Request.Title,
            Status = request.Request.Status,
            Visibility = request.Request.Visibility,
            OriginChannel = request.Request.OriginChannel,
            AiConfidenceScore = 0,
            FeedbackScore = 0,
            Sort = request.Request.Sort,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        space.Questions.Add(entity);
        dbContext.Questions.Add(entity);

        Apply(entity, request.Request, userId);
        var questionSnapshot = ActivityEntityMetadata.SnapshotQuestion(entity);
        ActivityAppender.AddQuestionActivity(
            entity,
            ActivityKind.QuestionCreated,
            actor,
            "Created",
            new Dictionary<string, object?>(StringComparer.Ordinal),
            questionSnapshot,
            ActivityEntityMetadata.QuestionContext(entity));

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private static void Apply(Common.Domain.Entities.Question entity, QuestionCreateRequestDto request, string userId)
    {
        QuestionRules.EnsureSupportedStatus(request.Status);

        entity.Title = request.Title;
        entity.Summary = request.Summary;
        entity.ContextNote = request.ContextNote;
        entity.Sort = request.Sort;
        entity.Status = request.Status;

        QuestionRules.EnsureVisibilityAllowed(entity, request.Visibility);
        entity.Visibility = request.Visibility;
        entity.UpdatedBy = userId;
    }
}
