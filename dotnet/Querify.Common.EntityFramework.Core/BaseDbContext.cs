using System.Net;
using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Core.Audit.DbContext.AuditableEntity;
using Querify.Common.EntityFramework.Core.Helpers;
using Querify.Common.EntityFramework.Core.SoftDelete.Abstractions;
using Querify.Common.EntityFramework.Core.SoftDelete.DbContext.SoftDelete;
using Querify.Common.EntityFramework.Core.Tenant.Abstractions;
using Querify.Common.EntityFramework.Core.Tenant.DbContext.Tenant;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Core.Attributes;
using Querify.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Querify.Common.EntityFramework.Core;

public abstract class BaseDbContext<TContext> : DbContext, ISoftDeleteFilterState, ITenantFilterState
    where TContext : DbContext
{
    private readonly IConfiguration _configuration;
    private readonly ISessionService _sessionService;
    private readonly ITenantConnectionStringProvider _tenantConnectionStringProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected BaseDbContext(
        DbContextOptions<TContext> options,
        ISessionService sessionService,
        IConfiguration configuration,
        ITenantConnectionStringProvider tenantConnectionStringProvider,
        IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _sessionService = sessionService;
        _configuration = configuration;
        _tenantConnectionStringProvider = tenantConnectionStringProvider;
        _httpContextAccessor = httpContextAccessor;
        TenantFiltersEnabled = ResolveTenantFiltersEnabled();
    }

    protected virtual IEnumerable<string> ConfigurationNamespaces => [];

    protected virtual bool UseTenantConnectionString => true;

    protected abstract ModuleEnum SessionModule { get; }

    protected Guid? SessionTenantId =>
        UseTenantConnectionString && TenantFiltersEnabled ? _sessionService.GetTenantId(SessionModule) : null;

    Guid? ITenantFilterState.SessionTenantId => SessionTenantId;

    public bool TenantFiltersEnabled { get; set; } = true;
    public bool SoftDeleteFiltersEnabled { get; set; } = true;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnBeforeSaveChangesRules();
        this.ApplySoftDeleteRules();
        this.ApplyAuditRules(ResolveUserId());
        NormalizeTrackedDateTimesToUtc();
        OnBeforeSaveChanges();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        OnBeforeSaveChangesRules();
        this.ApplySoftDeleteRules();
        this.ApplyAuditRules(ResolveUserId());
        NormalizeTrackedDateTimesToUtc();
        OnBeforeSaveChanges();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
    }

    protected virtual void OnBeforeSaveChanges()
    {
    }

    protected virtual void OnBeforeSaveChangesRules()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseCollation("Latin1_General_100_CI_AS_SC_UTF8");

        var configurationLoader = new EfConfigurationLoader<TContext>();
        foreach (var configurationNamespace in ConfigurationNamespaces)
        {
            configurationLoader.LoadFromNameSpace(modelBuilder, configurationNamespace);
        }

        modelBuilder.ApplySoftDeleteFilters(this);
        modelBuilder.ApplyTenantFilters(this);
        modelBuilder.ApplyTenantIndexes();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var connectionString = ResolveConnectionString();

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    private string GetDefaultConnectionString()
    {
        const string defaultConnectionStringName = "DefaultConnection";
        var defaultConnectionString = _configuration.GetConnectionString(defaultConnectionStringName);

        if (string.IsNullOrWhiteSpace(defaultConnectionString))
        {
            throw new ApiErrorException(
                $"Missing connection string '{defaultConnectionStringName}'.",
                errorCode: (int)HttpStatusCode.InternalServerError);
        }

        return defaultConnectionString;
    }

    private bool ResolveTenantFiltersEnabled()
    {
        var endpoint = _httpContextAccessor.HttpContext?.GetEndpoint();
        return endpoint?.Metadata.GetMetadata<SkipTenantAccessValidationAttribute>() is null;
    }

    private void NormalizeTrackedDateTimesToUtc()
    {
        foreach (var entry in ChangeTracker.Entries()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType != typeof(DateTime) &&
                    property.Metadata.ClrType != typeof(DateTime?))
                {
                    continue;
                }

                if (property.CurrentValue is DateTime currentValue)
                {
                    property.CurrentValue = NormalizeDateTimeToUtc(currentValue);
                }
            }
        }
    }

    private static DateTime NormalizeDateTimeToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private string? ResolveUserId()
    {
        var userId = _sessionService.GetUserId();
        return userId == Guid.Empty ? null : userId.ToString();
    }

    private sealed class ResetOnDispose : IDisposable
    {
        private readonly Action _reset;
        private bool _disposed;

        public ResetOnDispose(Action reset)
        {
            _reset = reset;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _reset();
            _disposed = true;
        }
    }

    private string ResolveConnectionString()
    {
        if (!UseTenantConnectionString)
        {
            return GetDefaultConnectionString();
        }

        var tenantId = _sessionService.GetTenantId(SessionModule);

        var tenantConnectionString = _tenantConnectionStringProvider.GetConnectionString(tenantId);

        if (string.IsNullOrWhiteSpace(tenantConnectionString))
        {
            throw new ApiErrorException(
                $"Tenant '{tenantId}' has an invalid connection string.",
                errorCode: (int)HttpStatusCode.InternalServerError);
        }

        return tenantConnectionString;
    }
}
