using BaseFaq.Common.EntityFramework.Core;
using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.QnA.Common.Persistence.QnADb;

public class QnADbContext(
    DbContextOptions<QnADbContext> options,
    ISessionService sessionService,
    IConfiguration configuration,
    ITenantConnectionStringProvider tenantConnectionStringProvider,
    IHttpContextAccessor httpContextAccessor)
    : BaseDbContext<QnADbContext>(
        options,
        sessionService,
        configuration,
        tenantConnectionStringProvider,
        httpContextAccessor)
{
    public DbSet<QuestionSpace> QuestionSpaces { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<KnowledgeSource> KnowledgeSources { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<QuestionSourceLink> QuestionSourceLinks { get; set; }
    public DbSet<AnswerSourceLink> AnswerSourceLinks { get; set; }
    public DbSet<ThreadActivity> ThreadActivities { get; set; }
    public DbSet<QuestionSpaceTopic> QuestionSpaceTopics { get; set; }
    public DbSet<QuestionSpaceSource> QuestionSpaceSources { get; set; }
    public DbSet<QuestionTopic> QuestionTopics { get; set; }

    protected override IEnumerable<string> ConfigurationNamespaces =>
    [
        "BaseFaq.QnA.Common.Persistence.QnADb.Configurations"
    ];

    protected override AppEnum SessionApp => AppEnum.QnA;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureTenantIntegrity();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantIntegrity();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureTenantIntegrity();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void EnsureTenantIntegrity()
    {
        var cache = new IntegrityLookupCache(this);

        ValidateQuestions(cache);
        ValidateAnswers(cache);
        ValidateKnowledgeSources();
        ValidateQuestionSourceLinks(cache);
        ValidateAnswerSourceLinks(cache);
        ValidateThreadActivities(cache);
        ValidateQuestionSpaceTopics(cache);
        ValidateQuestionSpaceSources(cache);
        ValidateQuestionTopics(cache);
    }

    private void ValidateQuestions(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<Question>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var question = entry.Entity;

            EnsureTenantMatch(question.TenantId, cache.GetQuestionSpaceTenant(question.SpaceId),
                nameof(Question.SpaceId));

            if (question.Visibility.IsPubliclyVisible() &&
                question.Status is not QuestionStatus.Open and not QuestionStatus.Answered
                    and not QuestionStatus.Validated)
                throw new InvalidOperationException(
                    $"Question '{question.Id}' cannot be public while in status '{question.Status}'.");

            if (question.AcceptedAnswerId is Guid acceptedAnswerId)
            {
                var acceptedAnswer = cache.GetAnswer(acceptedAnswerId);
                EnsureTenantMatch(question.TenantId, acceptedAnswer.TenantId, nameof(Question.AcceptedAnswerId));

                if (acceptedAnswer.QuestionId != question.Id)
                    throw new InvalidOperationException(
                        $"Question '{question.Id}' accepts answer '{acceptedAnswerId}' from a different thread.");

                if (acceptedAnswer.Status is not AnswerStatus.Published and not AnswerStatus.Validated)
                    throw new InvalidOperationException(
                        $"Question '{question.Id}' cannot accept answer '{acceptedAnswerId}' while it is in status '{acceptedAnswer.Status}'.");

                if (question.Visibility.IsPubliclyVisible() &&
                    !acceptedAnswer.Visibility.IsPubliclyVisible())
                    throw new InvalidOperationException(
                        $"Question '{question.Id}' cannot expose accepted answer '{acceptedAnswerId}' while the answer is not publicly visible.");
            }

            if (question.DuplicateOfQuestionId is Guid duplicateQuestionId)
            {
                var duplicateOfQuestionTenantId = cache.GetQuestionTenant(duplicateQuestionId);
                EnsureTenantMatch(question.TenantId, duplicateOfQuestionTenantId,
                    nameof(Question.DuplicateOfQuestionId));

                if (duplicateQuestionId == question.Id)
                    throw new InvalidOperationException(
                        $"Question '{question.Id}' cannot point to itself as a duplicate.");
            }
        }
    }

    private void ValidateAnswers(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<Answer>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var answer = entry.Entity;
            EnsureTenantMatch(answer.TenantId, cache.GetQuestionTenant(answer.QuestionId), nameof(Answer.QuestionId));

            if (answer.Visibility.IsPubliclyVisible() &&
                answer.Status is not AnswerStatus.Published and not AnswerStatus.Validated)
                throw new InvalidOperationException(
                    $"Answer '{answer.Id}' cannot be public while in status '{answer.Status}'.");

            if (answer.Visibility.IsPubliclyVisible() &&
                answer.Kind == AnswerKind.AiDraft &&
                answer.Status != AnswerStatus.Validated)
                throw new InvalidOperationException(
                    $"AI draft answer '{answer.Id}' must be validated before public exposure.");

        }
    }

    private void ValidateKnowledgeSources()
    {
        foreach (var entry in ChangeTracker.Entries<KnowledgeSource>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var source = entry.Entity;

            if (!source.Visibility.IsPubliclyVisible())
            {
                if (source.AllowsPublicCitation || source.AllowsPublicExcerpt)
                    throw new InvalidOperationException(
                        $"Knowledge source '{source.Id}' cannot allow public citation or excerpt reuse while not publicly visible.");

                continue;
            }

            if (source.Kind == SourceKind.InternalNote)
                throw new InvalidOperationException(
                    $"Knowledge source '{source.Id}' cannot expose internal notes publicly.");

            if (source.LastVerifiedAtUtc is null)
                throw new InvalidOperationException(
                    $"Knowledge source '{source.Id}' must be verified before public exposure.");
        }
    }

    private void ValidateQuestionSourceLinks(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<QuestionSourceLink>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            EnsureTenantMatch(link.TenantId, cache.GetQuestionTenant(link.QuestionId),
                nameof(QuestionSourceLink.QuestionId));
            EnsureTenantMatch(link.TenantId, cache.GetKnowledgeSourceTenant(link.SourceId),
                nameof(QuestionSourceLink.SourceId));
        }
    }

    private void ValidateAnswerSourceLinks(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<AnswerSourceLink>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            EnsureTenantMatch(link.TenantId, cache.GetAnswer(link.AnswerId).TenantId,
                nameof(AnswerSourceLink.AnswerId));
            EnsureTenantMatch(link.TenantId, cache.GetKnowledgeSourceTenant(link.SourceId),
                nameof(AnswerSourceLink.SourceId));
        }
    }

    private void ValidateThreadActivities(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<ThreadActivity>()
                     .Where(entry => entry.State != EntityState.Unchanged))
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
                throw new InvalidOperationException(
                    $"Thread activity '{entry.Entity.Id}' is append-only and cannot be modified or deleted.");

            var activity = entry.Entity;
            EnsureTenantMatch(activity.TenantId, cache.GetQuestionTenant(activity.QuestionId),
                nameof(ThreadActivity.QuestionId));

            if (activity.AnswerId is not Guid answerId) continue;

            var answer = cache.GetAnswer(answerId);
            EnsureTenantMatch(activity.TenantId, answer.TenantId, nameof(ThreadActivity.AnswerId));

            if (answer.QuestionId != activity.QuestionId)
                throw new InvalidOperationException(
                    $"Thread activity '{activity.Id}' references answer '{answerId}' from a different question.");
        }
    }

    private void ValidateQuestionSpaceTopics(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<QuestionSpaceTopic>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            EnsureTenantMatch(link.TenantId, cache.GetQuestionSpaceTenant(link.QuestionSpaceId),
                nameof(QuestionSpaceTopic.QuestionSpaceId));
            EnsureTenantMatch(link.TenantId, cache.GetTopicTenant(link.TopicId), nameof(QuestionSpaceTopic.TopicId));
        }
    }

    private void ValidateQuestionSpaceSources(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<QuestionSpaceSource>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            EnsureTenantMatch(link.TenantId, cache.GetQuestionSpaceTenant(link.QuestionSpaceId),
                nameof(QuestionSpaceSource.QuestionSpaceId));
            EnsureTenantMatch(link.TenantId, cache.GetKnowledgeSourceTenant(link.KnowledgeSourceId),
                nameof(QuestionSpaceSource.KnowledgeSourceId));
        }
    }

    private void ValidateQuestionTopics(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<QuestionTopic>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            EnsureTenantMatch(link.TenantId, cache.GetQuestionTenant(link.QuestionId),
                nameof(QuestionTopic.QuestionId));
            EnsureTenantMatch(link.TenantId, cache.GetTopicTenant(link.TopicId), nameof(QuestionTopic.TopicId));
        }
    }

    private static void EnsureTenantMatch(Guid expectedTenantId, Guid actualTenantId, string relationshipName)
    {
        if (actualTenantId != expectedTenantId)
            throw new InvalidOperationException(
                $"Cross-tenant relationship detected for '{relationshipName}'. Expected tenant '{expectedTenantId}' but found '{actualTenantId}'.");
    }

    private sealed class IntegrityLookupCache(QnADbContext dbContext)
    {
        private Dictionary<Guid, AnswerLookup>? _answers;
        private Dictionary<Guid, Guid>? _knowledgeSourceTenants;
        private Dictionary<Guid, Guid>? _questionSpaceTenants;
        private Dictionary<Guid, Guid>? _questionTenants;
        private Dictionary<Guid, Guid>? _topicTenants;

        public Guid GetQuestionSpaceTenant(Guid id)
        {
            return GetTenant<QuestionSpace>(id, nameof(QuestionSpace), ref _questionSpaceTenants);
        }

        public Guid GetQuestionTenant(Guid id)
        {
            return GetTenant<Question>(id, nameof(Question), ref _questionTenants);
        }

        public AnswerLookup GetAnswer(Guid id)
        {
            _answers ??= SeedAnswerCache();

            if (_answers.TryGetValue(id, out var cached)) return cached;

            var databaseLookup = dbContext.Answers
                .IgnoreQueryFilters()
                .Where(answer => answer.Id == id)
                .Select(answer => new AnswerLookup
                {
                    TenantId = answer.TenantId,
                    QuestionId = answer.QuestionId,
                    Status = answer.Status,
                    Visibility = answer.Visibility
                })
                .SingleOrDefault();

            if (databaseLookup is null)
                throw new InvalidOperationException($"Referenced {nameof(Answer)} '{id}' was not found.");

            _answers[id] = databaseLookup;
            return databaseLookup;
        }

        public Guid GetKnowledgeSourceTenant(Guid id)
        {
            return GetTenant<KnowledgeSource>(id, nameof(KnowledgeSource), ref _knowledgeSourceTenants);
        }

        public Guid GetTopicTenant(Guid id)
        {
            return GetTenant<Topic>(id, nameof(Topic), ref _topicTenants);
        }

        private Guid GetTenant<TEntity>(Guid id, string entityName, ref Dictionary<Guid, Guid>? cache)
            where TEntity : BaseEntity, IMustHaveTenant
        {
            cache ??= SeedTenantCache<TEntity>();

            if (cache.TryGetValue(id, out var tenantId)) return tenantId;

            var databaseLookup = dbContext.Set<TEntity>()
                .IgnoreQueryFilters()
                .Where(entity => entity.Id == id)
                .Select(entity => new TenantLookup
                {
                    TenantId = entity.TenantId
                })
                .SingleOrDefault();

            if (databaseLookup is null)
                throw new InvalidOperationException($"Referenced {entityName} '{id}' was not found.");

            cache[id] = databaseLookup.TenantId;
            return databaseLookup.TenantId;
        }

        private Dictionary<Guid, Guid> SeedTenantCache<TEntity>()
            where TEntity : BaseEntity, IMustHaveTenant
        {
            var cache = new Dictionary<Guid, Guid>();

            foreach (var entry in dbContext.ChangeTracker.Entries<TEntity>()
                         .Where(entry => entry.State != EntityState.Deleted))
                cache[entry.Entity.Id] = entry.Entity.TenantId;

            return cache;
        }

        private Dictionary<Guid, AnswerLookup> SeedAnswerCache()
        {
            var cache = new Dictionary<Guid, AnswerLookup>();

            foreach (var entry in dbContext.ChangeTracker.Entries<Answer>()
                         .Where(entry => entry.State != EntityState.Deleted))
                cache[entry.Entity.Id] = new AnswerLookup
                {
                    TenantId = entry.Entity.TenantId,
                    QuestionId = entry.Entity.QuestionId,
                    Status = entry.Entity.Status,
                    Visibility = entry.Entity.Visibility
                };

            return cache;
        }
    }

    private sealed class TenantLookup
    {
        public required Guid TenantId { get; init; }
    }

    private sealed class AnswerLookup
    {
        public required Guid TenantId { get; init; }
        public required Guid QuestionId { get; init; }
        public required AnswerStatus Status { get; init; }
        public required VisibilityScope Visibility { get; init; }
    }
}
