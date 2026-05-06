using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Question;
using MediatR;

namespace Querify.QnA.Public.Business.Question.Queries.GetQuestionList;

public sealed class QuestionsGetQuestionListQuery : IRequest<PagedResultDto<QuestionDto>>
{
    public required QuestionGetAllRequestDto Request { get; set; }
}