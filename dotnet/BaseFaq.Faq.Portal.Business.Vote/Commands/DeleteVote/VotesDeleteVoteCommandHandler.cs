using BaseFaq.Faq.Common.Persistence.FaqDb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.Vote.Commands.DeleteVote;

public class VotesDeleteVoteCommandHandler(FaqDbContext dbContext)
    : IRequestHandler<VotesDeleteVoteCommand>
{
    public async Task Handle(VotesDeleteVoteCommand request, CancellationToken cancellationToken)
    {
        var vote = await dbContext.Votes.FirstOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (vote is null)
        {
            return;
        }

        var tenantId = vote.TenantId;
        var faqItemAnswerId = vote.FaqItemAnswerId;

        dbContext.Votes.Remove(vote);
        await dbContext.SaveChangesAsync(cancellationToken);

        var faqItemAnswer = await dbContext.FaqItemAnswers
            .FirstOrDefaultAsync(answer => answer.TenantId == tenantId && answer.Id == faqItemAnswerId, cancellationToken);
        if (faqItemAnswer is null)
        {
            return;
        }

        faqItemAnswer.VoteScore = await dbContext.Votes
            .Where(entity => entity.TenantId == tenantId && entity.FaqItemAnswerId == faqItemAnswerId)
            .CountAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
