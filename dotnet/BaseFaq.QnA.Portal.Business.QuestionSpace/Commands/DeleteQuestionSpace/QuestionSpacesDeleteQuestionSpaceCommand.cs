using MediatR;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.DeleteQuestionSpace;

public sealed class QuestionSpacesDeleteQuestionSpaceCommand : IRequest
{
    public Guid Id { get; set; }
}