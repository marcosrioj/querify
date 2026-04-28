using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.AddSource;

public sealed class QuestionsAddSourceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsAddSourceCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsAddSourceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var question = await dbContext.Questions
            .Include(entity => entity.Sources)
            .ThenInclude(link => link.Source)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.QuestionId,
                cancellationToken);
        var source = await dbContext.Sources
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.SourceId,
                cancellationToken);

        if (question is null)
            throw new ApiErrorException($"Question '{request.Request.QuestionId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (source is null)
            throw new ApiErrorException($"Source '{request.Request.SourceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        EnsureSourceSupportsVisibility(question.Visibility, source, request.Request.Role);

        var link = new QuestionSourceLink
        {
            TenantId = tenantId,
            QuestionId = question.Id,
            Question = question,
            SourceId = source.Id,
            Source = source,
            Role = request.Request.Role,
            Order = request.Request.Order,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        question.Sources.Add(link);
        source.Questions.Add(link);
        dbContext.QuestionSourceLinks.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        return link.Id;
    }

    private static void EnsureSourceSupportsVisibility(
        VisibilityScope questionVisibility,
        Source source,
        SourceRole role)
    {
        if (questionVisibility is not VisibilityScope.Public and not VisibilityScope.PublicIndexed) return;

        if (role is SourceRole.Citation or SourceRole.CanonicalReference &&
            (source.Visibility is not VisibilityScope.Public and not VisibilityScope.PublicIndexed ||
             !source.AllowsPublicCitation))
            throw new InvalidOperationException(
                "Public citations require a publicly visible source that explicitly allows citation.");
    }
}