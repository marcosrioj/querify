using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Dtos.ThreadActivity;
using BaseFaq.Models.QnA.Dtos.Topic;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionDetailDto : QuestionDto
{
    public AnswerDto? AcceptedAnswer { get; set; }
    public IReadOnlyList<AnswerDto> Answers { get; set; } = [];
    public IReadOnlyList<TopicDto> Topics { get; set; } = [];
    public IReadOnlyList<QuestionSourceLinkDto> Sources { get; set; } = [];
    public IReadOnlyList<ThreadActivityDto> Activity { get; set; } = [];
}
