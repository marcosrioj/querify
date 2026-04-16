using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.AddTag;

public sealed class QuestionsAddTagCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionsAddTagCommand, Guid>
{
    public async Task<Guid> Handle(QuestionsAddTagCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var question = await dbContext.Questions
            .Include(entity => entity.Tags)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.QuestionId,
                cancellationToken);
        var tag = await dbContext.Tags
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.TagId,
                cancellationToken);

        if (question is null)
            throw new ApiErrorException($"Question '{request.Request.QuestionId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (tag is null)
            throw new ApiErrorException($"Tag '{request.Request.TagId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (question.Tags.All(link => link.TagId != tag.Id))
            question.Tags.Add(new QuestionTag
            {
                TenantId = tenantId,
                QuestionId = question.Id,
                Question = question,
                TagId = tag.Id,
                Tag = tag,
                CreatedBy = userId,
                UpdatedBy = userId
            });

        await dbContext.SaveChangesAsync(cancellationToken);
        return question.Tags.Single(link => link.TagId == tag.Id).Id;
    }
}
