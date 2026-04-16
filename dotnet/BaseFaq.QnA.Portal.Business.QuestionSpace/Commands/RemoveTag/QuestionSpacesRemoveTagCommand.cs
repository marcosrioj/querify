using MediatR;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.RemoveTag;

public sealed class QuestionSpacesRemoveTagCommand : IRequest
{
    public Guid QuestionSpaceId { get; set; }
    public Guid TagId { get; set; }
}