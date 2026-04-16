using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AnswerSourceLinkEntity = BaseFaq.QnA.Common.Persistence.QnADb.Entities.AnswerSourceLink;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.AddSource;

public sealed class AnswersAddSourceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersAddSourceCommand, Guid>
{
    public async Task<Guid> Handle(AnswersAddSourceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var answer = await dbContext.Answers
            .Include(entity => entity.Question)
            .Include(entity => entity.Sources)
            .ThenInclude(link => link.Source)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.AnswerId,
                cancellationToken);
        var source = await dbContext.KnowledgeSources
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.SourceId,
                cancellationToken);

        if (answer is null)
            throw new ApiErrorException($"Answer '{request.Request.AnswerId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (source is null)
            throw new ApiErrorException(
                $"Knowledge source '{request.Request.SourceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        EnsureSourceSupportsVisibility(answer.Visibility, source, request.Request.Role);

        var link = new AnswerSourceLinkEntity
        {
            TenantId = tenantId,
            AnswerId = answer.Id,
            Answer = answer,
            SourceId = source.Id,
            Source = source,
            Role = request.Request.Role,
            Order = request.Request.Order,
            ConfidenceScore = request.Request.ConfidenceScore,
            IsPrimary = request.Request.IsPrimary,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        answer.Sources.Add(link);
        source.Answers.Add(link);
        dbContext.AnswerSourceLinks.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        return link.Id;
    }

    private static void EnsureSourceSupportsVisibility(
        VisibilityScope answerVisibility,
        KnowledgeSource source,
        SourceRole role)
    {
        if (answerVisibility is not VisibilityScope.Public and not VisibilityScope.PublicIndexed) return;

        if (role is SourceRole.Citation or SourceRole.CanonicalReference &&
            (source.Visibility is not VisibilityScope.Public and not VisibilityScope.PublicIndexed ||
             !source.AllowsPublicCitation))
            throw new InvalidOperationException(
                "Public citations require a publicly visible source that explicitly allows citation.");
    }
}
