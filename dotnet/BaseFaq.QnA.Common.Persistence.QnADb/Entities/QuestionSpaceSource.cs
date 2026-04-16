using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.QnA.Common.Persistence.QnADb.Guards;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Persistence helper that links a question space to a curated source.
/// </summary>
public sealed class QuestionSpaceSource : BaseEntity, IMustHaveTenant
{
    private QuestionSpaceSource()
    {
    }

    public QuestionSpaceSource(QuestionSpace questionSpace, KnowledgeSource knowledgeSource, string? createdBy = null)
    {
        ArgumentNullException.ThrowIfNull(questionSpace);
        ArgumentNullException.ThrowIfNull(knowledgeSource);

        Id = Guid.NewGuid();
        TenantId = DomainGuards.TenantIdOf(questionSpace, nameof(questionSpace));
        DomainGuards.InitializeAudit(this, createdBy);
        DomainGuards.EnsureSameTenant(questionSpace, knowledgeSource, "question space to curated source");

        QuestionSpaceId = questionSpace.Id;
        QuestionSpace = questionSpace;
        KnowledgeSourceId = knowledgeSource.Id;
        KnowledgeSource = knowledgeSource;
    }

    public Guid TenantId { get; private set; }
    public Guid QuestionSpaceId { get; private set; }
    public QuestionSpace QuestionSpace { get; private set; } = null!;
    public Guid KnowledgeSourceId { get; private set; }
    public KnowledgeSource KnowledgeSource { get; private set; } = null!;
}
