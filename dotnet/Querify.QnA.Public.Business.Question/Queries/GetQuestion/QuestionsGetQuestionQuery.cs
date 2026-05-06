using Querify.Models.QnA.Dtos.Question;
using MediatR;

namespace Querify.QnA.Public.Business.Question.Queries.GetQuestion;

public sealed class QuestionsGetQuestionQuery : IRequest<QuestionDetailDto>
{
    public required Guid Id { get; set; }
    public required QuestionGetRequestDto Request { get; set; }
}