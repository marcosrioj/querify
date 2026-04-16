using BaseFaq.Models.QnA.Dtos.Answer;

namespace BaseFaq.QnA.Public.Business.Vote.Abstractions;

public interface IVoteService
{
    Task<Guid> Create(AnswerVoteCreateRequestDto dto, CancellationToken token);
}
