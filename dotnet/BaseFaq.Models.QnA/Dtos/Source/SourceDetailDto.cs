namespace BaseFaq.Models.QnA.Dtos.Source;

public class SourceDetailDto : SourceDto
{
    public IReadOnlyList<SourceSpaceRelationshipDto> Spaces { get; set; } = [];
    public IReadOnlyList<SourceQuestionRelationshipDto> Questions { get; set; } = [];
    public IReadOnlyList<SourceAnswerRelationshipDto> Answers { get; set; } = [];
}
