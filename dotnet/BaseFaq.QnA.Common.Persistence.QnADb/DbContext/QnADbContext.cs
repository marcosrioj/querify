using BaseFaq.Common.EntityFramework.Core;
using BaseFaq.Common.EntityFramework.Core.AutoHistory.DbContext.AutoHistory;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.QnA.Common.Persistence.QnADb.DbContext;

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

    protected override ModuleEnum SessionModule => ModuleEnum.QnA;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.EnableAutoHistory();

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnBeforeSaveChanges()
    {
        this.EnsureAutoHistory();
    }

    protected override void OnBeforeSaveChangesRules()
    {
        EnsureActivityIdentity();
        EnsureTenantIntegrity();
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
        var cache = new TenantIntegrityLookupCache(this);

        this.EnsureSpaceTenantIntegrity();
        this.EnsureQuestionTenantIntegrity(cache);
        this.EnsureAnswerTenantIntegrity(cache);
        this.EnsureSourceTenantIntegrity();
        this.EnsureQuestionSourceLinkTenantIntegrity(cache);
        this.EnsureAnswerSourceLinkTenantIntegrity(cache);
        this.EnsureActivityTenantIntegrity(cache);
        this.EnsureSpaceTagTenantIntegrity(cache);
        this.EnsureSpaceSourceTenantIntegrity(cache);
        this.EnsureQuestionTagTenantIntegrity(cache);
    }
}
