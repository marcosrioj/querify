using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Portal.Business.Vote.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.Vote.Commands.UpdateVote;

public class VotesUpdateVoteCommandHandler(
    FaqDbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<VotesUpdateVoteCommand>
{
    public async Task Handle(VotesUpdateVoteCommand request, CancellationToken cancellationToken)
    {
        var vote = await dbContext.Votes.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (vote is null)
        {
            throw new ApiErrorException(
                $"Vote '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        var identity = VoteRequestContext.GetIdentity(sessionService, httpContextAccessor);
        var previousFaqItemAnswerId = vote.FaqItemAnswerId;

        await GetFaqItemAnswerOrThrowAsync(vote.TenantId, request.FaqItemAnswerId, cancellationToken);

        vote.UserPrint = identity.UserPrint;
        vote.Ip = identity.Ip;
        vote.UserAgent = identity.UserAgent;
        vote.FaqItemAnswerId = request.FaqItemAnswerId;

        dbContext.Votes.Update(vote);
        await dbContext.SaveChangesAsync(cancellationToken);

        await RecalculateVoteScoreAsync(vote.TenantId, previousFaqItemAnswerId, cancellationToken);
        if (previousFaqItemAnswerId != request.FaqItemAnswerId)
        {
            await RecalculateVoteScoreAsync(vote.TenantId, request.FaqItemAnswerId, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task GetFaqItemAnswerOrThrowAsync(
        Guid tenantId,
        Guid faqItemAnswerId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.FaqItemAnswers
            .AnyAsync(answer => answer.TenantId == tenantId && answer.Id == faqItemAnswerId, cancellationToken);

        if (!exists)
        {
            throw new ApiErrorException(
                $"FAQ item answer '{faqItemAnswerId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }
    }

    private async Task RecalculateVoteScoreAsync(Guid tenantId, Guid faqItemAnswerId, CancellationToken cancellationToken)
    {
        var faqItemAnswer = await dbContext.FaqItemAnswers
            .FirstOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == faqItemAnswerId, cancellationToken);
        if (faqItemAnswer is null)
        {
            return;
        }

        faqItemAnswer.VoteScore = await dbContext.Votes
            .Where(vote => vote.TenantId == tenantId && vote.FaqItemAnswerId == faqItemAnswerId)
            .CountAsync(cancellationToken);
    }
}
