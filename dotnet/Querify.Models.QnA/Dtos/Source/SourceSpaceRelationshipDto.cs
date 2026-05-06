using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Source;

public class SourceSpaceRelationshipDto
{
    public required Guid Id { get; set; }
    public required Guid SpaceId { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Summary { get; set; }
    public required SpaceStatus Status { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public required bool AcceptsQuestions { get; set; }
    public required bool AcceptsAnswers { get; set; }
    public required int QuestionCount { get; set; }
}
