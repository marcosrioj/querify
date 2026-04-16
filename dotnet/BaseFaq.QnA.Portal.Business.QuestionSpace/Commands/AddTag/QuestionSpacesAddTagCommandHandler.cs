using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.AddTag;

public sealed class QuestionSpacesAddTagCommandHandler(
    QnADbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<QuestionSpacesAddTagCommand, Guid>
{
    public async Task<Guid> Handle(QuestionSpacesAddTagCommand request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.QnA);
        var userId = sessionService.GetUserId().ToString();
        var space = await dbContext.QuestionSpaces
            .Include(entity => entity.Tags)
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.QuestionSpaceId,
                cancellationToken);
        var tag = await dbContext.Tags
            .SingleOrDefaultAsync(entity => entity.TenantId == tenantId && entity.Id == request.Request.TagId,
                cancellationToken);

        if (space is null)
            throw new ApiErrorException(
                $"Question space '{request.Request.QuestionSpaceId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (tag is null)
            throw new ApiErrorException($"Tag '{request.Request.TagId}' was not found.",
                (int)HttpStatusCode.NotFound);

        if (space.Tags.All(link => link.TagId != tag.Id))
            space.Tags.Add(new QuestionSpaceTag
            {
                TenantId = tenantId,
                QuestionSpaceId = space.Id,
                QuestionSpace = space,
                TagId = tag.Id,
                Tag = tag,
                CreatedBy = userId,
                UpdatedBy = userId
            });

        await dbContext.SaveChangesAsync(cancellationToken);

        return space.Tags.Single(link => link.TagId == tag.Id).Id;
    }
}
