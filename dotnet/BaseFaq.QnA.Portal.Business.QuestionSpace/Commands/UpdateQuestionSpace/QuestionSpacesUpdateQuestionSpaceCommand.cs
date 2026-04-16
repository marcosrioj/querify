using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.UpdateQuestionSpace;

public sealed class QuestionSpacesUpdateQuestionSpaceCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public required QuestionSpaceUpdateRequestDto Request { get; set; }
}
