using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Space.Commands.DeleteSpace;

public sealed class SpacesDeleteSpaceCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<SpacesDeleteSpaceCommand>
{
    public async Task Handle(SpacesDeleteSpaceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Spaces
            .SingleOrDefaultAsync(space => space.TenantId == tenantId && space.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Space '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        var answers = await dbContext.Answers
            .Where(answer => answer.TenantId == tenantId && answer.Question.SpaceId == entity.Id)
            .ToListAsync(cancellationToken);
        var questions = await dbContext.Questions
            .Where(question => question.TenantId == tenantId && question.SpaceId == entity.Id)
            .ToListAsync(cancellationToken);

        dbContext.Answers.RemoveRange(answers);
        dbContext.Questions.RemoveRange(questions);
        dbContext.Spaces.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
