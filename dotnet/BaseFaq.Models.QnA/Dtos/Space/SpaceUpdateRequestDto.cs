using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Space;

public class SpaceUpdateRequestDto
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string Language { get; set; }
    public string? Summary { get; set; }
    public required SpaceKind Kind { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public required bool AcceptsQuestions { get; set; }
    public required bool AcceptsAnswers { get; set; }
}
