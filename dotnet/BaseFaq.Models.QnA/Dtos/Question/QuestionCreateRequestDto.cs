using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionCreateRequestDto
{
    public Guid SpaceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ContextNote { get; set; }
    public string? ThreadSummary { get; set; }
    public QuestionKind Kind { get; set; } = QuestionKind.Curated;
    public QuestionStatus Status { get; set; } = QuestionStatus.Draft;
    public VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;
    public ChannelKind OriginChannel { get; set; } = ChannelKind.Manual;
    public string? Language { get; set; }
    public string? ProductScope { get; set; }
    public string? JourneyScope { get; set; }
    public string? AudienceScope { get; set; }
    public string? ContextKey { get; set; }
    public string? OriginUrl { get; set; }
    public string? OriginReference { get; set; }
    public int ConfidenceScore { get; set; }
}
