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

public class QnADbContext : BaseDbContext<QnADbContext>
{
    public QnADbContext(
        DbContextOptions<QnADbContext> options,
        ISessionService sessionService,
        IConfiguration configuration,
        ITenantConnectionStringProvider tenantConnectionStringProvider,
        IHttpContextAccessor httpContextAccessor)
        : base(
            options,
            sessionService,
            configuration,
            tenantConnectionStringProvider,
            httpContextAccessor)
    {
    }

    public DbSet<Space> Spaces { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Source> Sources { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<QuestionSourceLink> QuestionSourceLinks { get; set; }
    public DbSet<AnswerSourceLink> AnswerSourceLinks { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<SpaceTag> SpaceTags { get; set; }
    public DbSet<SpaceSource> SpaceSources { get; set; }
    public DbSet<QuestionTag> QuestionTags { get; set; }

    protected override IEnumerable<string> ConfigurationNamespaces =>
    [
        "BaseFaq.QnA.Common.Persistence.QnADb.Configurations"
    ];

    protected override AppEnum SessionApp => AppEnum.QnA;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureActivityIdentity();
        EnsureTenantIntegrity();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnsureActivityIdentity();
        EnsureTenantIntegrity();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureActivityIdentity();
        EnsureTenantIntegrity();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void EnsureActivityIdentity()
    {
        foreach (var entry in ChangeTracker.Entries<Activity>()
                     .Where(entry => entry.State == EntityState.Added))
        {
            var activity = entry.Entity;

            if (string.IsNullOrWhiteSpace(activity.UserPrint))
                throw new InvalidOperationException(
                    $"Activity '{activity.Id}' must include a resolved '{nameof(Activity.UserPrint)}'.");

            if (string.IsNullOrWhiteSpace(activity.Ip))
                throw new InvalidOperationException(
                    $"Activity '{activity.Id}' must include a resolved '{nameof(Activity.Ip)}'.");

            if (string.IsNullOrWhiteSpace(activity.UserAgent))
                throw new InvalidOperationException(
                    $"Activity '{activity.Id}' must include a resolved '{nameof(Activity.UserAgent)}'.");
        }
    }

    private void EnsureTenantIntegrity()
    {
        var cache = new IntegrityLookupCache(this);

        ValidateQuestions(cache);
        ValidateAnswers(cache);
        ValidateSources();
        ValidateQuestionSourceLinks(cache);
        ValidateAnswerSourceLinks(cache);
        ValidateActivities(cache);
        ValidateSpaceTags(cache);
        ValidateSpaceSources(cache);
        ValidateQuestionTags(cache);
    }

    private void ValidateQuestions(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<Question>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var question = entry.Entity;

            EnsureTenantMatch(question.TenantId, cache.GetSpaceTenant(question.SpaceId),
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

    private void ValidateSources()
    {
        foreach (var entry in ChangeTracker.Entries<Source>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var source = entry.Entity;

            if (!source.Visibility.IsPubliclyVisible())
            {
                if (source.AllowsPublicCitation || source.AllowsPublicExcerpt)
                    throw new InvalidOperationException(
                        $"Source '{source.Id}' cannot allow public citation or excerpt reuse while not publicly visible.");

                continue;
            }

            if (source.Kind == SourceKind.InternalNote)
                throw new InvalidOperationException(
                    $"Source '{source.Id}' cannot expose internal notes publicly.");

            if (source.LastVerifiedAtUtc is null)
                throw new InvalidOperationException(
                    $"Source '{source.Id}' must be verified before public exposure.");
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
            EnsureTenantMatch(link.TenantId, cache.GetSourceTenant(link.SourceId),
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
            EnsureTenantMatch(link.TenantId, cache.GetSourceTenant(link.SourceId),
                nameof(AnswerSourceLink.SourceId));
        }
    }

    private void ValidateActivities(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<Activity>()
                     .Where(entry => entry.State != EntityState.Unchanged))
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
                throw new InvalidOperationException(
                    $"Activity '{entry.Entity.Id}' is append-only and cannot be modified or deleted.");

            var activity = entry.Entity;
            EnsureTenantMatch(activity.TenantId, cache.GetQuestionTenant(activity.QuestionId),
                nameof(Activity.QuestionId));

            if (activity.AnswerId is not Guid answerId) continue;

            var answer = cache.GetAnswer(answerId);
            EnsureTenantMatch(activity.TenantId, answer.TenantId, nameof(Activity.AnswerId));

            if (answer.QuestionId != activity.QuestionId)
                throw new InvalidOperationException(
                    $"Activity '{activity.Id}' references answer '{answerId}' from a different question.");
        }
    }

    private void ValidateSpaceTags(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<SpaceTag>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            EnsureTenantMatch(link.TenantId, cache.GetSpaceTenant(link.SpaceId),
                nameof(SpaceTag.SpaceId));
            EnsureTenantMatch(link.TenantId, cache.GetTagTenant(link.TagId), nameof(SpaceTag.TagId));
        }
    }

    private void ValidateSpaceSources(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<SpaceSource>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            EnsureTenantMatch(link.TenantId, cache.GetSpaceTenant(link.SpaceId),
                nameof(SpaceSource.SpaceId));
            EnsureTenantMatch(link.TenantId, cache.GetSourceTenant(link.SourceId),
                nameof(SpaceSource.SourceId));
        }
    }

    private void ValidateQuestionTags(IntegrityLookupCache cache)
    {
        foreach (var entry in ChangeTracker.Entries<QuestionTag>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            EnsureTenantMatch(link.TenantId, cache.GetQuestionTenant(link.QuestionId),
                nameof(QuestionTag.QuestionId));
            EnsureTenantMatch(link.TenantId, cache.GetTagTenant(link.TagId), nameof(QuestionTag.TagId));
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
        private Dictionary<Guid, Guid>? _questionTenants;
        private Dictionary<Guid, Guid>? _sourceTenants;
        private Dictionary<Guid, Guid>? _spaceTenants;
        private Dictionary<Guid, Guid>? _tagTenants;

        public Guid GetSpaceTenant(Guid id)
        {
            return GetTenant<Space>(id, nameof(Space), ref _spaceTenants);
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

        public Guid GetSourceTenant(Guid id)
        {
            return GetTenant<Source>(id, nameof(Source), ref _sourceTenants);
        }

        public Guid GetTagTenant(Guid id)
        {
            return GetTenant<Tag>(id, nameof(Tag), ref _tagTenants);
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