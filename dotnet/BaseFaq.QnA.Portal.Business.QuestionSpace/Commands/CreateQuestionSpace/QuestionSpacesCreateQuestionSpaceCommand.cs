using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.CreateQuestionSpace;

public sealed class QuestionSpacesCreateQuestionSpaceCommand : IRequest<Guid>
{
    public required QuestionSpaceCreateRequestDto Request { get; set; }
}