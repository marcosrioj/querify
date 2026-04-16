using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestionList;

public sealed class QuestionsGetQuestionListQuery : IRequest<PagedResultDto<QuestionDto>>
{
    public required QuestionGetAllRequestDto Request { get; set; }
}
