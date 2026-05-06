using Querify.Models.QnA.Dtos.Answer;

namespace Querify.QnA.Public.Business.Vote.Abstractions;

public interface IVoteService
{
    Task<Guid> Create(AnswerVoteCreateRequestDto dto, CancellationToken token);
}