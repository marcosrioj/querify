using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Question.Queries.GetQuestionByKey;

public sealed class QuestionsGetQuestionByKeyQuery : IRequest<QuestionDetailDto>
{
    public required string Key { get; set; }
    public required QuestionGetRequestDto Request { get; set; }
}