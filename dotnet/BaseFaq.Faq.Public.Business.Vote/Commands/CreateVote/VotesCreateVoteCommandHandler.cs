using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Constants;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Public.Business.Vote.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Public.Business.Vote.Commands.CreateVote;

public class VotesCreateVoteCommandHandler(
    FaqDbContext dbContext,
    IClientKeyContextService clientKeyContextService,
    ITenantClientKeyResolver tenantClientKeyResolver,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<VotesCreateVoteCommand, Guid>
{
    public async Task<Guid> Handle(VotesCreateVoteCommand request, CancellationToken cancellationToken)
    {
        var identity = VoteRequestContext.GetIdentity(httpContextAccessor);
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);

        var existing = await dbContext.Votes
            .FirstOrDefaultAsync(vote =>
                vote.TenantId == tenantId &&
                vote.FaqItemAnswerId == request.FaqItemAnswerId &&
                vote.UserPrint == identity.UserPrint,
                cancellationToken);
        if (existing is not null)
        {
            return existing.Id;
        }

        var faqItemAnswer = await GetFaqItemAnswerOrThrowAsync(tenantId, request.FaqItemAnswerId, cancellationToken);
        var vote = new Common.Persistence.FaqDb.Entities.Vote
        {
            UserPrint = identity.UserPrint,
            Ip = identity.Ip,
            UserAgent = identity.UserAgent,
            TenantId = tenantId,
            FaqItemAnswerId = request.FaqItemAnswerId
        };

        await dbContext.Votes.AddAsync(vote, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        faqItemAnswer.VoteScore = await dbContext.Votes
            .Where(entity => entity.TenantId == tenantId && entity.FaqItemAnswerId == request.FaqItemAnswerId)
            .CountAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return vote.Id;
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }

    private async Task<Common.Persistence.FaqDb.Entities.FaqItemAnswer> GetFaqItemAnswerOrThrowAsync(
        Guid tenantId,
        Guid faqItemAnswerId,
        CancellationToken cancellationToken)
    {
        var faqItemAnswer = await dbContext.FaqItemAnswers
            .FirstOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == faqItemAnswerId, cancellationToken);

        if (faqItemAnswer is null)
        {
            throw new ApiErrorException(
                $"FAQ item answer '{faqItemAnswerId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return faqItemAnswer;
    }
}
