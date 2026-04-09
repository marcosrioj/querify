using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Vote;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Vote.Queries.GetVoteList;

public sealed class VotesGetVoteListQuery : IRequest<PagedResultDto<VoteDto>>
{
    public required VoteGetAllRequestDto Request { get; set; }
}
