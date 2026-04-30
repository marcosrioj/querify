using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Space;

public class SpaceDto
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Summary { get; set; }
    public required string Language { get; set; }
    public required SpaceStatus Status { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public required bool AcceptsQuestions { get; set; }
    public required bool AcceptsAnswers { get; set; }
    public required int QuestionCount { get; set; }
    public DateTime? LastUpdatedAtUtc { get; set; }
}
