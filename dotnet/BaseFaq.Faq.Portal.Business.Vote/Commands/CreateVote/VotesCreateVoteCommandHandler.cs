using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Portal.Business.Vote.Helpers;
using BaseFaq.Models.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.Vote.Commands.CreateVote;

public class VotesCreateVoteCommandHandler(
    FaqDbContext dbContext,
    ISessionService sessionService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<VotesCreateVoteCommand, Guid>
{
    public async Task<Guid> Handle(VotesCreateVoteCommand request, CancellationToken cancellationToken)
    {
        var identity = VoteRequestContext.GetIdentity(sessionService, httpContextAccessor);
        var tenantId = sessionService.GetTenantId(AppEnum.Faq);
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

        faqItemAnswer.VoteScore = await CountVotesAsync(tenantId, request.FaqItemAnswerId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return vote.Id;
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

    private Task<int> CountVotesAsync(Guid tenantId, Guid faqItemAnswerId, CancellationToken cancellationToken)
    {
        return dbContext.Votes
            .Where(vote => vote.TenantId == tenantId && vote.FaqItemAnswerId == faqItemAnswerId)
            .CountAsync(cancellationToken);
    }
}
