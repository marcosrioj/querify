using BaseFaq.AI.Persistence.AiDb.Entities;
using BaseFaq.Common.EntityFramework.Core;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.AI.Persistence.AiDb;

public class AiDbContext(
    DbContextOptions<AiDbContext> options,
    ISessionService sessionService,
    IConfiguration configuration,
    ITenantConnectionStringProvider tenantConnectionStringProvider,
    IHttpContextAccessor httpContextAccessor)
    : BaseDbContext<AiDbContext>(
        options,
        sessionService,
        configuration,
        tenantConnectionStringProvider,
        httpContextAccessor)
{
    public DbSet<GenerationJob> GenerationJobs { get; set; } = null!;
    public DbSet<GenerationArtifact> GenerationArtifacts { get; set; } = null!;
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; } = null!;

    protected override IEnumerable<string> ConfigurationNamespaces =>
    [
        "BaseFaq.AI.Persistence.AiDb.Configurations"
    ];

    protected override bool UseTenantConnectionString => false;
    protected override AppEnum SessionApp => AppEnum.Faq;
}