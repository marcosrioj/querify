using BaseFaq.Models.QnA.Dtos.Question;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Question.Commands.CreateReport;

public sealed class QuestionsCreateReportCommand : IRequest<Guid>
{
    public required QuestionReportCreateRequestDto Request { get; set; }
}