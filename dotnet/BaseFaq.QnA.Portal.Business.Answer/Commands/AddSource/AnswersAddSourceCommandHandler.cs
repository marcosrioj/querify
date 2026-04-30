using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Domain.BusinessRules.Answers;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands.AddSource;

public sealed class AnswersAddSourceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersAddSourceCommand, Guid>
{
    public async Task<Guid> Handle(AnswersAddSourceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var answer = await dbContext.Answers
            .Include(entity => entity.Question)
            .Include(entity => entity.Sources)
            .ThenInclude(link => link.Source)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.AnswerId,
                cancellationToken);
        var source = await dbContext.Sources
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.SourceId,
                cancellationToken);

        if (answer is null)
            throw new ApiErrorException($"Answer '{request.Request.AnswerId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (source is null)
            throw new ApiErrorException(
                $"Source '{request.Request.SourceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        var link = AnswerRules.CreateSourceLink(
            answer,
            source,
            request.Request.Role,
            request.Request.Order,
            tenantId,
            userId);

        answer.Sources.Add(link);
        source.Answers.Add(link);
        dbContext.AnswerSourceLinks.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        return link.Id;
    }
}
