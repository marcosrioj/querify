using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.QnA.Dtos.Activity;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Activity.Queries.GetActivity;

public sealed class ActivitiesGetActivityQueryHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<ActivitiesGetActivityQuery, ActivityDto>
{
    public async Task<ActivityDto> Handle(ActivitiesGetActivityQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Activities.AsNoTracking()
            .Where(activity => activity.TenantId == tenantId && activity.Id == request.Id)
            .Select(activity => new ActivityDto
            {
                Id = activity.Id,
                TenantId = activity.TenantId,
                QuestionId = activity.QuestionId,
                QuestionTitle = activity.Question.Title,
                AnswerId = activity.AnswerId,
                AnswerHeadline = activity.Answer == null ? null : activity.Answer.Headline,
                Kind = activity.Kind,
                ActorKind = activity.ActorKind,
                ActorLabel = activity.ActorLabel,
                UserPrint = activity.UserPrint,
                Ip = activity.Ip,
                UserAgent = activity.UserAgent,
                Notes = activity.Notes,
                MetadataJson = activity.MetadataJson,
                OccurredAtUtc = activity.OccurredAtUtc,
                CreatedAtUtc = activity.CreatedDate ?? activity.OccurredAtUtc,
                LastUpdatedAtUtc = activity.UpdatedDate ?? activity.CreatedDate ?? activity.OccurredAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? throw new ApiErrorException(
                $"Activity '{request.Id}' was not found.",
                (int)HttpStatusCode.NotFound)
            : entity;
    }
}
