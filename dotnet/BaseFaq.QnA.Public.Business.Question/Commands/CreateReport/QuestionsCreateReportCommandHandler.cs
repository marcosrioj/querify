using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Public.Business.Question.Commands.CreateReport;

public sealed class QuestionsCreateReportCommandHandler(
    QnADbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    ISessionService sessionService,
    IClaimService claimService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<QuestionsCreateReportCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsCreateReportCommand request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new ApiErrorException(
            "HttpContext is missing from the current request.",
            (int)HttpStatusCode.Unauthorized);
        var identity = ActivityIdentityResolver.ResolveActivityIdentity(
            sessionService,
            ActivityRequestInfo.GetRequiredIp(httpContext),
            ActivityRequestInfo.GetRequiredUserAgent(httpContext),
            claimService.GetExternalUserId());
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);
        var question = await dbContext.Questions
            .Include(entity => entity.Activities)
            .SingleOrDefaultAsync(
                entity =>
                    entity.TenantId == tenantId &&
                    entity.Id == request.Request.QuestionId &&
                    (entity.Visibility == VisibilityScope.Public ||
                     entity.Visibility == VisibilityScope.PublicIndexed) &&
                    (entity.Status == QuestionStatus.Open || entity.Status == QuestionStatus.Answered ||
                     entity.Status == QuestionStatus.Validated),
                cancellationToken);

        if (question is null)
            throw new ApiErrorException(
                $"Question '{request.Request.QuestionId}' was not found.",
                (int)HttpStatusCode.NotFound);

        Answer? answer = null;
        if (request.Request.AnswerId is Guid answerId)
        {
            answer = await dbContext.Answers
                .SingleOrDefaultAsync(
                    entity =>
                        entity.TenantId == tenantId &&
                        entity.Id == answerId &&
                        entity.QuestionId == question.Id &&
                        (entity.Visibility == VisibilityScope.Public ||
                         entity.Visibility == VisibilityScope.PublicIndexed) &&
                        (entity.Status == AnswerStatus.Published || entity.Status == AnswerStatus.Validated),
                    cancellationToken);

            if (answer is null)
                throw new ApiErrorException(
                    $"Answer '{answerId}' was not found.",
                    (int)HttpStatusCode.NotFound);
        }

        var activity = new Activity
        {
            TenantId = question.TenantId,
            QuestionId = question.Id,
            Question = question,
            AnswerId = answer?.Id,
            Answer = answer,
            Kind = ActivityKind.ReportReceived,
            ActorKind = ActorKind.Customer,
            ActorLabel = identity.UserPrint,
            UserPrint = identity.UserPrint,
            Ip = identity.Ip,
            UserAgent = identity.UserAgent,
            Notes = request.Request.Notes,
            MetadataJson = ActivitySignals.CreateReportMetadata(
                identity.UserPrint,
                identity.Ip,
                identity.UserAgent,
                request.Request.Reason),
            OccurredAtUtc = DateTime.UtcNow,
            CreatedBy = identity.UserPrint,
            UpdatedBy = identity.UserPrint
        };

        question.Activities.Add(activity);
        question.LastActivityAtUtc = activity.OccurredAtUtc;
        dbContext.Activities.Add(activity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return activity.Id;
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }
}