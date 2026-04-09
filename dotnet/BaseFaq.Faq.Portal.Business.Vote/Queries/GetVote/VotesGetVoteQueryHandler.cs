using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Models.Faq.Dtos.Vote;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Faq.Portal.Business.Vote.Queries.GetVote;

public class VotesGetVoteQueryHandler(FaqDbContext dbContext) : IRequestHandler<VotesGetVoteQuery, VoteDto?>
{
    public async Task<VoteDto?> Handle(VotesGetVoteQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Votes
            .AsNoTracking()
            .Where(entity => entity.Id == request.Id)
            .Select(vote => new VoteDto
            {
                Id = vote.Id,
                UserPrint = vote.UserPrint,
                Ip = vote.Ip,
                UserAgent = vote.UserAgent,
                FaqItemAnswerId = vote.FaqItemAnswerId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
