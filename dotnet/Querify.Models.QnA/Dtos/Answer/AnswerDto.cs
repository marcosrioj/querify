using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Answer;

public class AnswerDto
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required Guid QuestionId { get; set; }
    public required string Headline { get; set; }
    public string? Body { get; set; }
    public required AnswerKind Kind { get; set; }
    public required AnswerStatus Status { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public string? ContextNote { get; set; }
    public string? AuthorLabel { get; set; }
    public required int AiConfidenceScore { get; set; }
    public required int Score { get; set; }
    public required int Sort { get; set; }
    public required bool IsAccepted { get; set; }
    public required bool IsOfficial { get; set; }
    public DateTime? LastUpdatedAtUtc { get; set; }
    public required int VoteScore { get; set; }
    public IReadOnlyList<AnswerSourceLinkDto> Sources { get; set; } = [];
}
