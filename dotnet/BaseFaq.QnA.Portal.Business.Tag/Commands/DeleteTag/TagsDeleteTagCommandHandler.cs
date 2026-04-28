using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Tag.Commands.DeleteTag;

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