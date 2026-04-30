using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Domain.BusinessRules.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.Question.Commands.CreateQuestion;

public sealed class QuestionsCreateQuestionCommandHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    ISessionService sessionService,
    IClaimService claimService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsCreateQuestionCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsCreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var actor = ActivityActorResolver.ResolvePublicActor(
            sessionService,
            claimService,
            httpContextAccessor,
            ActorKind.Customer);
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var space = await dbContext.Spaces
            .Include(entity => entity.Questions)
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.Id == request.Request.SpaceId &&
                    entity.Visibility == VisibilityScope.Public &&
                    entity.Status == SpaceStatus.Active,
                cancellationToken);

        if (space is null)
            throw new ApiErrorException(
                $"Space '{request.Request.SpaceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (!space.AcceptsQuestions)
            throw new ApiErrorException(
                "This space is not accepting questions.",
                (int)HttpStatusCode.UnprocessableEntity);

        var entity = new Common.Domain.Entities.Question
        {
            TenantId = tenantId,
            SpaceId = space.Id,
            Space = space,
            Title = request.Request.Title,
            Summary = request.Request.Summary,
            ContextNote = request.Request.ContextNote,
            OriginChannel = request.Request.OriginChannel,
            AiConfidenceScore = 0,
            FeedbackScore = 0,
            Sort = request.Request.Sort,
            Status = QuestionStatus.Active,
            Visibility = VisibilityScope.Authenticated,
            CreatedBy = "public",
            UpdatedBy = "public"
        };

        space.Questions.Add(entity);
        dbContext.Questions.Add(entity);

        var questionSnapshot = SnapshotQuestion(entity);
        ActivityAppender.AddQuestionActivity(
            entity,
            ActivityKind.QuestionCreated,
            actor,
            "Created",
            new Dictionary<string, object?>(StringComparer.Ordinal),
            questionSnapshot,
            QuestionContext(entity));

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }

    private static Dictionary<string, object?> SnapshotQuestion(Common.Domain.Entities.Question entity)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["Id"] = entity.Id,
            ["TenantId"] = entity.TenantId,
            ["SpaceId"] = entity.SpaceId,
            ["Title"] = entity.Title,
            ["Summary"] = entity.Summary,
            ["ContextNote"] = entity.ContextNote,
            ["Status"] = entity.Status.ToString(),
            ["Visibility"] = entity.Visibility.ToString(),
            ["OriginChannel"] = entity.OriginChannel.ToString(),
            ["AiConfidenceScore"] = entity.AiConfidenceScore,
            ["FeedbackScore"] = entity.FeedbackScore,
            ["Sort"] = entity.Sort,
            ["AcceptedAnswerId"] = entity.AcceptedAnswerId
        };
    }

    private static Dictionary<string, object?> QuestionContext(Common.Domain.Entities.Question entity)
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["QuestionId"] = entity.Id,
            ["SpaceId"] = entity.SpaceId,
            ["Status"] = entity.Status.ToString(),
            ["Visibility"] = entity.Visibility.ToString()
        };
    }
}
