using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.DeleteQuestionSpace;

public sealed class QuestionSpacesDeleteQuestionSpaceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesDeleteQuestionSpaceCommand>
{
    public async Task Handle(QuestionSpacesDeleteQuestionSpaceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var entity = await dbContext.QuestionSpaces
            .SingleOrDefaultAsync(space => space.TenantId == tenantId && space.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new ApiErrorException($"Question space '{request.Id}' was not found.", errorCode: (int)HttpStatusCode.NotFound);
        }

        dbContext.QuestionSpaces.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
