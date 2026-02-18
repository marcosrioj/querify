using BaseFaq.Models.Faq.Dtos.Vote;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Vote.Queries.GetVote;

public sealed class VotesGetVoteQuery : IRequest<VoteDto?>
{
    public required Guid Id { get; set; }
}