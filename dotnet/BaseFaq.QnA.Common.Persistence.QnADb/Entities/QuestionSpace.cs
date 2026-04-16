using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

public class QuestionSpace : BaseEntity, IMustHaveTenant
{
    public const int MaxNameLength = 200;
    public const int MaxKeyLength = 160;
    public const int MaxSummaryLength = 2000;
    public const int MaxLanguageLength = 50;
    public const int MaxProductScopeLength = 200;
    public const int MaxJourneyScopeLength = 200;

    public required string Name { get; set; }
    public required string Key { get; set; }
    public string? Summary { get; set; }
    public required string DefaultLanguage { get; set; }
    public SpaceKind Kind { get; set; } = SpaceKind.CuratedKnowledge;
    public VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;
    public ModerationPolicy ModerationPolicy { get; set; } = ModerationPolicy.PreModeration;
    public SearchMarkupMode SearchMarkupMode { get; set; } = SearchMarkupMode.Off;
    public string? ProductScope { get; set; }
    public string? JourneyScope { get; set; }
    public bool AcceptsQuestions { get; set; }
    public bool AcceptsAnswers { get; set; }
    public bool RequiresQuestionReview { get; set; } = true;
    public bool RequiresAnswerReview { get; set; } = true;
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? LastValidatedAtUtc { get; set; }
    public required Guid TenantId { get; set; }

    public ICollection<Question> Questions { get; set; } = [];
    public ICollection<QuestionSpaceTopic> QuestionSpaceTopics { get; set; } = [];
    public ICollection<QuestionSpaceSource> QuestionSpaceSources { get; set; } = [];
}
