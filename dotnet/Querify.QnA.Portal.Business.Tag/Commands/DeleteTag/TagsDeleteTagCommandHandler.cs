using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Tag.Commands.DeleteTag;

public sealed class TagsDeleteTagCommandHandler(QnADbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TagsDeleteTagCommand>
{
    public async Task Handle(TagsDeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(ModuleEnum.QnA);
        var entity = await dbContext.Tags
            .SingleOrDefaultAsync(tag => tag.TenantId == tenantId && tag.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new ApiErrorException($"Tag '{request.Id}' was not found.", (int)HttpStatusCode.NotFound);

        dbContext.Tags.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}