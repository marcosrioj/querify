using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Dtos.Activity;
using BaseFaq.Models.QnA.Dtos.Tag;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionDetailDto : QuestionDto
{
    public AnswerDto? AcceptedAnswer { get; set; }
    public IReadOnlyList<AnswerDto> Answers { get; set; } = [];
    public IReadOnlyList<TagDto> Tags { get; set; } = [];
    public IReadOnlyList<QuestionSourceLinkDto> Sources { get; set; } = [];
    public IReadOnlyList<ActivityDto> Activity { get; set; } = [];
}
