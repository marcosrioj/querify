using Querify.Common.EntityFramework.Core;
using Querify.Common.EntityFramework.Core.AutoHistory.DbContext.AutoHistory;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Querify.QnA.Common.Persistence.QnADb.DbContext;

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
    public DbSet<SourceUploadedOutboxMessage> SourceUploadedOutboxMessages { get; set; }

    protected override IEnumerable<string> ConfigurationNamespaces =>
    [
        "Querify.QnA.Common.Persistence.QnADb.Configurations"
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
                throw new ApiErrorException(
                    $"Activity '{activity.Id}' must include a resolved '{nameof(Activity.UserPrint)}'.",
                    (int)HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(activity.Ip))
                throw new ApiErrorException(
                    $"Activity '{activity.Id}' must include a resolved '{nameof(Activity.Ip)}'.",
                    (int)HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(activity.UserAgent))
                throw new ApiErrorException(
                    $"Activity '{activity.Id}' must include a resolved '{nameof(Activity.UserAgent)}'.",
                    (int)HttpStatusCode.BadRequest);
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
