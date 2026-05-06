using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Space;

public class SpaceCreateRequestDto
{
    public required string Name { get; set; }
    public string? Slug { get; set; }
    public required string Language { get; set; }
    public string? Summary { get; set; }
    public required SpaceStatus Status { get; set; }
    public VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;
    public required bool AcceptsQuestions { get; set; }
    public required bool AcceptsAnswers { get; set; }
}
