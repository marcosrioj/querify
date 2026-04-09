using BaseFaq.Common.EntityFramework.Core;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.Faq.Common.Persistence.FaqDb;

public class FaqDbContext(
    DbContextOptions<FaqDbContext> options,
    ISessionService sessionService,
    IConfiguration configuration,
    ITenantConnectionStringProvider tenantConnectionStringProvider,
    IHttpContextAccessor httpContextAccessor)
    : BaseDbContext<FaqDbContext>(
        options,
        sessionService,
        configuration,
        tenantConnectionStringProvider,
        httpContextAccessor)
{
    public DbSet<Common.Persistence.FaqDb.Entities.Faq> Faqs { get; set; }

    public DbSet<FaqItem> FaqItems { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ContentRef> ContentRefs { get; set; }
    public DbSet<FaqTag> FaqTags { get; set; }
    public DbSet<FaqContentRef> FaqContentRefs { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<FaqItemAnswer> FaqItemAnswers { get; set; }
    public DbSet<Vote> Votes { get; set; }

    protected override IEnumerable<string> ConfigurationNamespaces =>
    [
        "BaseFaq.Faq.Common.Persistence.FaqDb.Configurations"
    ];

    protected override AppEnum SessionApp => AppEnum.Faq;
}
