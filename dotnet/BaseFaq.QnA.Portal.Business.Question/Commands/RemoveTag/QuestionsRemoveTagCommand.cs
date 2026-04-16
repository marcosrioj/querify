using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.RemoveTag;

public sealed class QuestionsRemoveTagCommand : IRequest
{
    public Guid QuestionId { get; set; }
    public Guid TagId { get; set; }
}