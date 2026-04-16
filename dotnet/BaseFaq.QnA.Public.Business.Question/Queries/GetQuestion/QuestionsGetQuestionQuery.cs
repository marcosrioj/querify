using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Question.Queries.GetQuestion;

public sealed class QuestionsGetQuestionQuery : IRequest<QuestionDetailDto>
{
    public Guid Id { get; set; }
    public required QuestionGetRequestDto Request { get; set; }
}
