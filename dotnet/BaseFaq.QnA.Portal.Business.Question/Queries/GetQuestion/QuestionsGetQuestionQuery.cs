using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestion;

public sealed class QuestionsGetQuestionQuery : IRequest<QuestionDetailDto>
{
    public Guid Id { get; set; }
}
