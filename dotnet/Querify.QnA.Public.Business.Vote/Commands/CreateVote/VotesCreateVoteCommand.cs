using Querify.Models.QnA.Dtos.Answer;
using MediatR;

namespace Querify.QnA.Public.Business.Vote.Commands.CreateVote;

public sealed class VotesCreateVoteCommand : IRequest<Guid>
{
    public required AnswerVoteCreateRequestDto Request { get; set; }
}