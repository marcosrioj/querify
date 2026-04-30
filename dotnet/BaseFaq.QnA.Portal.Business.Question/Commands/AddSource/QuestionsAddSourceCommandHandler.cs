using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Domain.BusinessRules.Questions;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
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

        var link = QuestionRules.CreateSourceLink(
            question,
            source,
            request.Request.Role,
            request.Request.Order,
            tenantId,
            userId);

        question.Sources.Add(link);
        source.Questions.Add(link);
        dbContext.QuestionSourceLinks.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        return link.Id;
    }
}
