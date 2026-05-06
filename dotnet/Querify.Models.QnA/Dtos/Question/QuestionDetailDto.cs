using Querify.Models.QnA.Dtos.Answer;
using Querify.Models.QnA.Dtos.Activity;
using Querify.Models.QnA.Dtos.Tag;

namespace Querify.Models.QnA.Dtos.Question;

public class QuestionDetailDto : QuestionDto
{
    public AnswerDto? AcceptedAnswer { get; set; }
    public IReadOnlyList<AnswerDto> Answers { get; set; } = [];
    public IReadOnlyList<TagDto> Tags { get; set; } = [];
    public IReadOnlyList<QuestionSourceLinkDto> Sources { get; set; } = [];
    public IReadOnlyList<ActivityDto> Activity { get; set; } = [];
}
