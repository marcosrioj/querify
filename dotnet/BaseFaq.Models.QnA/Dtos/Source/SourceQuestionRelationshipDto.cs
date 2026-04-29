using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Source;

public class SourceQuestionRelationshipDto
{
    public required Guid Id { get; set; }
    public required Guid QuestionId { get; set; }
    public required Guid SpaceId { get; set; }
    public required string SpaceSlug { get; set; }
    public required string Title { get; set; }
    public string? Summary { get; set; }
    public required QuestionStatus Status { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public required SourceRole Role { get; set; }
    public required int Order { get; set; }
    public DateTime? LastActivityAtUtc { get; set; }
}
