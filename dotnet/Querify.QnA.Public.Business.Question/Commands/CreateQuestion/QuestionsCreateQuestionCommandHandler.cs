using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Core.Constants;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Activities;
using Querify.QnA.Common.Domain.BusinessRules.Spaces;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Public.Business.Question.Commands.CreateQuestion;

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

        SpaceRules.EnsureAcceptsQuestions(space);

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

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }

}
