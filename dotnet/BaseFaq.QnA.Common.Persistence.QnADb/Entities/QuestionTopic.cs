using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.QnA.Common.Persistence.QnADb.Guards;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Persistence helper that links a question thread to a reusable topic.
/// </summary>
public sealed class QuestionTopic : BaseEntity, IMustHaveTenant
{
    private QuestionTopic()
    {
    }

    public QuestionTopic(Question question, Topic topic, string? createdBy = null)
    {
        ArgumentNullException.ThrowIfNull(question);
        ArgumentNullException.ThrowIfNull(topic);

        Id = Guid.NewGuid();
        TenantId = DomainGuards.TenantIdOf(question, nameof(question));
        DomainGuards.InitializeAudit(this, createdBy);
        DomainGuards.EnsureSameTenant(question, topic, "question to topic");

        QuestionId = question.Id;
        Question = question;
        TopicId = topic.Id;
        Topic = topic;
    }

    public Guid TenantId { get; private set; }
    public Guid QuestionId { get; private set; }
    public Question Question { get; private set; } = null!;
    public Guid TopicId { get; private set; }
    public Topic Topic { get; private set; } = null!;
}
