using BaseFaq.Models.Faq.Enums;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Vote.Commands.UpdateVote;

public sealed class VotesUpdateVoteCommand : IRequest
{
    public required Guid Id { get; set; }
    public required bool Like { get; set; }
    public UnLikeReason? UnLikeReason { get; set; }
    public required Guid FaqItemId { get; set; }
}