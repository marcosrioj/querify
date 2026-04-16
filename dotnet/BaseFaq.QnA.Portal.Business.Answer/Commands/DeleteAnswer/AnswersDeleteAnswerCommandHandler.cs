using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Answer.Commands;

public sealed class AnswersDeleteAnswerCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<AnswersDeleteAnswerCommand>
{
    public async Task Handle(AnswersDeleteAnswerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await dbContext.Answers
            .SingleOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new ApiErrorException($"Answer '{request.Id}' was not found.", errorCode: (int)HttpStatusCode.NotFound);
        }

        dbContext.Answers.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
