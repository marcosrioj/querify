using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.QnA.Common.Persistence.QnADb.Guards;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Persistence helper that links a question space to a reusable topic.
/// </summary>
public sealed class QuestionSpaceTopic : BaseEntity, IMustHaveTenant
{
    private QuestionSpaceTopic()
    {
    }

    public QuestionSpaceTopic(QuestionSpace questionSpace, Topic topic, string? createdBy = null)
    {
        ArgumentNullException.ThrowIfNull(questionSpace);
        ArgumentNullException.ThrowIfNull(topic);

        Id = Guid.NewGuid();
        TenantId = DomainGuards.TenantIdOf(questionSpace, nameof(questionSpace));
        DomainGuards.InitializeAudit(this, createdBy);
        DomainGuards.EnsureSameTenant(questionSpace, topic, "question space to topic");

        QuestionSpaceId = questionSpace.Id;
        QuestionSpace = questionSpace;
        TopicId = topic.Id;
        Topic = topic;
    }

    public Guid TenantId { get; private set; }
    public Guid QuestionSpaceId { get; private set; }
    public QuestionSpace QuestionSpace { get; private set; } = null!;
    public Guid TopicId { get; private set; }
    public Topic Topic { get; private set; } = null!;
}
