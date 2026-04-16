namespace BaseFaq.Models.QnA.Dtos.QuestionSpace;

public class QuestionSpaceDetailDto : QuestionSpaceDto
{
    public IReadOnlyList<BaseFaq.Models.QnA.Dtos.Topic.TopicDto> Topics { get; set; } = [];
    public IReadOnlyList<BaseFaq.Models.QnA.Dtos.KnowledgeSource.KnowledgeSourceDto> CuratedSources { get; set; } = [];
}
