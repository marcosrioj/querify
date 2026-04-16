using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionDto
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required Guid SpaceId { get; set; }
    public required string SpaceKey { get; set; } = string.Empty;
    public required string Title { get; set; } = string.Empty;
    public required string Key { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ContextNote { get; set; }
    public required QuestionKind Kind { get; set; }
    public required QuestionStatus Status { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public required ChannelKind OriginChannel { get; set; }
    public string? Language { get; set; }
    public string? ProductScope { get; set; }
    public string? JourneyScope { get; set; }
    public string? AudienceScope { get; set; }
    public string? ContextKey { get; set; }
    public string? OriginUrl { get; set; }
    public string? OriginReference { get; set; }
    public string? ThreadSummary { get; set; }
    public required int ConfidenceScore { get; set; }
    public required int RevisionNumber { get; set; }
    public Guid? AcceptedAnswerId { get; set; }
    public Guid? DuplicateOfQuestionId { get; set; }
    public DateTime? AnsweredAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime? ValidatedAtUtc { get; set; }
    public DateTime? LastActivityAtUtc { get; set; }
    public required int FeedbackScore { get; set; }
}
