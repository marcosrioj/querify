using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Commands.RemoveTag;

public sealed class QuestionsRemoveTagCommand : IRequest
{
    public required Guid QuestionId { get; set; }
    public required Guid TagId { get; set; }
}