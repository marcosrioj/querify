using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands;

public sealed class QuestionSpacesAddTopicCommand : IRequest<Guid>
{
    public required QuestionSpaceTopicCreateRequestDto Request { get; set; }
}
