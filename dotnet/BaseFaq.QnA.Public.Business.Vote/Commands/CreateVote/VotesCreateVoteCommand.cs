using BaseFaq.Models.QnA.Dtos.Answer;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Vote.Commands.CreateVote;

public sealed class VotesCreateVoteCommand : IRequest<Guid>
{
    public required AnswerVoteCreateRequestDto Request { get; set; }
}
