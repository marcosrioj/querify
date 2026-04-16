using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

public class Question : BaseEntity, IMustHaveTenant
{
    public const int MaxTitleLength = 1000;
    public const int MaxKeyLength = 200;
    public const int MaxSummaryLength = 500;
    public const int MaxContextNoteLength = 2000;
    public const int MaxLanguageLength = 50;
    public const int MaxProductScopeLength = 200;
    public const int MaxJourneyScopeLength = 200;
    public const int MaxAudienceScopeLength = 200;
    public const int MaxContextKeyLength = 200;
    public const int MaxOriginUrlLength = 1000;
    public const int MaxOriginReferenceLength = 250;
    public const int MaxThreadSummaryLength = 4000;
    public required string Title { get; set; } = null!;
    public required string Key { get; set; } = null!;
    public string? Summary { get; set; }
    public string? ContextNote { get; set; }
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
    public string? ThreadSummary { get; set; }
    public int ConfidenceScore { get; set; }
    public int RevisionNumber { get; set; }
    public required Guid SpaceId { get; set; }
    public QuestionSpace Space { get; set; } = null!;
    public Guid? AcceptedAnswerId { get; set; }
    public Answer? AcceptedAnswer { get; set; }
    public Guid? DuplicateOfQuestionId { get; set; }
    public Question? DuplicateOfQuestion { get; set; }
    public DateTime? AnsweredAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime? ValidatedAtUtc { get; set; }
    public DateTime? LastActivityAtUtc { get; set; }
    public ICollection<Question> DuplicateQuestions { get; set; } = [];
    public ICollection<Answer> Answers { get; set; } = [];
    public ICollection<QuestionSourceLink> Sources { get; set; } = [];
    public ICollection<QuestionTopic> QuestionTopics { get; set; } = [];
    public ICollection<ThreadActivity> Activity { get; set; } = [];

    public required Guid TenantId { get; set; }
}