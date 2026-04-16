using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SpaceId { get; set; }
    public string SpaceKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ContextNote { get; set; }
    public QuestionKind Kind { get; set; }
    public QuestionStatus Status { get; set; }
    public VisibilityScope Visibility { get; set; }
    public ChannelKind OriginChannel { get; set; }
    public string? Language { get; set; }
    public string? ProductScope { get; set; }
    public string? JourneyScope { get; set; }
    public string? AudienceScope { get; set; }
    public string? ContextKey { get; set; }
    public string? OriginUrl { get; set; }
    public string? OriginReference { get; set; }
    public string? ThreadSummary { get; set; }
    public int ConfidenceScore { get; set; }
    public int RevisionNumber { get; set; }
    public Guid? AcceptedAnswerId { get; set; }
    public Guid? DuplicateOfQuestionId { get; set; }
    public DateTime? AnsweredAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime? ValidatedAtUtc { get; set; }
    public DateTime? LastActivityAtUtc { get; set; }
    public int FeedbackScore { get; set; }
}
