using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestionEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Question;
using ThreadActivityEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.ThreadActivity;

namespace BaseFaq.QnA.Public.Business.Question.Commands;

public sealed class QuestionsCreateQuestionCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsCreateQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsCreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var space = await dbContext.QuestionSpaces
            .Include(entity => entity.Questions)
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.Id == request.Request.SpaceId &&
                    (entity.Visibility == VisibilityScope.Public || entity.Visibility == VisibilityScope.PublicIndexed),
                cancellationToken);

        if (space is null)
        {
            throw new ApiErrorException(
                $"Question space '{request.Request.SpaceId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        if (!space.AcceptsQuestions)
        {
            throw new ApiErrorException(
                "This question space is not accepting questions.",
                errorCode: (int)HttpStatusCode.UnprocessableEntity);
        }

        var entity = new QuestionEntity
        {
            TenantId = tenantId,
            SpaceId = space.Id,
            Space = space,
            Title = request.Request.Title,
            Key = request.Request.Key,
            Summary = request.Request.Summary,
            ContextNote = request.Request.ContextNote,
            ThreadSummary = request.Request.ThreadSummary,
            Kind = request.Request.Kind,
            OriginChannel = request.Request.OriginChannel,
            Language = request.Request.Language,
            ProductScope = request.Request.ProductScope,
            JourneyScope = request.Request.JourneyScope,
            AudienceScope = request.Request.AudienceScope,
            ContextKey = request.Request.ContextKey,
            OriginUrl = request.Request.OriginUrl,
            OriginReference = request.Request.OriginReference,
            ConfidenceScore = request.Request.ConfidenceScore,
            RevisionNumber = 1,
            Status = space.RequiresQuestionReview ? QuestionStatus.PendingReview : QuestionStatus.Open,
            Visibility = VisibilityScope.Internal,
            CreatedBy = "public",
            UpdatedBy = "public"
        };

        space.Questions.Add(entity);
        dbContext.Questions.Add(entity);

        var createdActivity = new ThreadActivityEntity
        {
            TenantId = entity.TenantId,
            QuestionId = entity.Id,
            Question = entity,
            Kind = ActivityKind.QuestionCreated,
            ActorKind = ActorKind.Customer,
            ActorLabel = "public",
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = "public",
            UpdatedBy = "public"
        };
        entity.Activity.Add(createdActivity);
        entity.LastActivityAtUtc = createdActivity.OccurredAtUtc;
        dbContext.ThreadActivities.Add(createdActivity);

        var submittedActivity = new ThreadActivityEntity
        {
            TenantId = entity.TenantId,
            QuestionId = entity.Id,
            Question = entity,
            Kind = ActivityKind.QuestionSubmitted,
            ActorKind = ActorKind.Customer,
            ActorLabel = "public",
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = "public",
            UpdatedBy = "public"
        };
        entity.Activity.Add(submittedActivity);
        entity.LastActivityAtUtc = submittedActivity.OccurredAtUtc;
        dbContext.ThreadActivities.Add(submittedActivity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
