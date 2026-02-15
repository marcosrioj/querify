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
        ValidateUnlikeReason(request);
        var identity = ResolveVoteIdentity();
        var tenantId = await ResolveTenantIdAndSetContextAsync(cancellationToken);

        var existing = await dbContext.Votes
            .FirstOrDefaultAsync(
                vote => vote.TenantId == tenantId &&
                        vote.FaqItemId == request.FaqItemId &&
                        vote.UserPrint == identity.UserPrint,
                cancellationToken);

        if (existing is not null)
        {
            if (existing.Like == request.Like)
            {
                return existing.Id;
            }

            return await UpdateExistingVoteAsync(request, tenantId, identity, existing, cancellationToken);
        }

        return await CreateVoteAsync(request, tenantId, identity, cancellationToken);
    }

    private static void ValidateUnlikeReason(VotesCreateVoteCommand request)
    {
        if (!request.Like && request.UnLikeReason is null)
        {
            throw new ApiErrorException(
                "UnLikeReason is required when Like is false.",
                errorCode: (int)HttpStatusCode.UnprocessableEntity);
        }
    }

    private VoteRequestIdentity ResolveVoteIdentity()
    {
        return VoteRequestContext.GetIdentity(httpContextAccessor);
    }

    private async Task<Guid> ResolveTenantIdAndSetContextAsync(CancellationToken cancellationToken)
    {
        var clientKey = clientKeyContextService.GetRequiredClientKey();
        var tenantId = await tenantClientKeyResolver.ResolveTenantId(clientKey, cancellationToken);
        httpContextAccessor.HttpContext?.Items[TenantContextKeys.TenantIdItemKey] = tenantId;
        return tenantId;
    }

    private async Task<Guid> UpdateExistingVoteAsync(
        VotesCreateVoteCommand request,
        Guid tenantId,
        VoteRequestIdentity identity,
        Common.Persistence.FaqDb.Entities.Vote existing,
        CancellationToken cancellationToken)
    {
        var faqItem = await GetFaqItemOrThrowAsync(tenantId, request.FaqItemId, cancellationToken);

        existing.Like = request.Like;
        existing.Ip = identity.Ip;
        existing.UserAgent = identity.UserAgent;
        existing.UnLikeReason = request.UnLikeReason;
        faqItem.VoteScore += request.Like ? 2 : -2;
        await dbContext.SaveChangesAsync(cancellationToken);

        return existing.Id;
    }

    private async Task<Guid> CreateVoteAsync(
        VotesCreateVoteCommand request,
        Guid tenantId,
        VoteRequestIdentity identity,
        CancellationToken cancellationToken)
    {
        var faqItemForCreate = await GetFaqItemOrThrowAsync(tenantId, request.FaqItemId, cancellationToken);
        var vote = new Common.Persistence.FaqDb.Entities.Vote
        {
            Like = request.Like,
            UserPrint = identity.UserPrint,
            Ip = identity.Ip,
            UserAgent = identity.UserAgent,
            UnLikeReason = request.UnLikeReason,
            TenantId = tenantId,
            FaqItemId = request.FaqItemId
        };

        await dbContext.Votes.AddAsync(vote, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        faqItemForCreate.VoteScore = await dbContext.Votes
            .Where(v => v.TenantId == tenantId && v.FaqItemId == request.FaqItemId)
            .SumAsync(v => v.Like ? 1 : -1, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return vote.Id;
    }

    private async Task<Common.Persistence.FaqDb.Entities.FaqItem> GetFaqItemOrThrowAsync(
        Guid tenantId,
        Guid faqItemId,
        CancellationToken cancellationToken)
    {
        var faqItem = await dbContext.FaqItems
            .FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Id == faqItemId, cancellationToken);

        if (faqItem is null)
        {
            throw new ApiErrorException(
                $"FAQ item '{faqItemId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return faqItem;
    }
}