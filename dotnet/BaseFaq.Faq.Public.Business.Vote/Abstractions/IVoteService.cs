using BaseFaq.Models.Faq.Dtos.Vote;

namespace BaseFaq.Faq.Public.Business.Vote.Abstractions;

public interface IVoteService
{
    Task<Guid> Vote(VoteCreateRequestDto requestDto, CancellationToken token);
}
