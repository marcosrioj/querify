using System.Linq.Expressions;
using System.Net;
using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Common.EntityFramework.Core.Helpers;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Attributes;
using BaseFaq.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.Common.EntityFramework.Core;

public abstract class BaseDbContext<TContext> : DbContext where TContext : DbContext
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

    public bool TenantFiltersEnabled { get; set; } = true;
    public bool SoftDeleteFiltersEnabled { get; set; } = true;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplySoftDeleteRules();
        ApplyAuditRules();
        NormalizeTrackedDateTimesToUtc();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplySoftDeleteRules();
        ApplyAuditRules();
        NormalizeTrackedDateTimesToUtc();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDeleteRules();
        ApplyAuditRules();
        NormalizeTrackedDateTimesToUtc();
        return base.SaveChangesAsync(cancellationToken);
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

        ApplySoftDeleteFilters(modelBuilder);
        ApplyTenantFilters(modelBuilder);
        ApplyTenantIndexes(modelBuilder);
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

    private void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var filter = BuildSoftDeleteFilterExpression(entityType.ClrType);
            if (filter is null)
            {
                continue;
            }

            ApplyQueryFilter(modelBuilder, entityType.ClrType, filter);
        }
    }

    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var filter = BuildTenantFilterExpression(entityType.ClrType);
            if (filter is null)
            {
                continue;
            }

            ApplyQueryFilter(modelBuilder, entityType.ClrType, filter);
        }
    }

    private static void ApplyQueryFilter(ModelBuilder modelBuilder, Type entityType, LambdaExpression filter)
    {
        var entity = modelBuilder.Entity(entityType);
        var declaredFilters = entity.Metadata.GetDeclaredQueryFilters();

        if (!declaredFilters.Any())
        {
            entity.HasQueryFilter(filter);
            return;
        }

        var parameter = Expression.Parameter(entityType, "e");
        var combinedLeft = CombineFilters(declaredFilters, parameter);
        var right = ReplaceParameter(filter, parameter);
        var combined = Expression.Lambda(Expression.AndAlso(combinedLeft, right), parameter);

        entity.HasQueryFilter(combined);
    }

    private static Expression CombineFilters(
        IEnumerable<IQueryFilter> filters,
        ParameterExpression parameter)
    {
        Expression? combined = null;

        foreach (var queryFilter in filters)
        {
            if (queryFilter.Expression != null)
            {
                var filterBody = ReplaceParameter(queryFilter.Expression, parameter);
                combined = combined is null ? filterBody : Expression.AndAlso(combined, filterBody);
            }
        }

        return combined ?? Expression.Constant(true);
    }

    private static Expression ReplaceParameter(LambdaExpression expression, ParameterExpression parameter)
    {
        return new ParameterReplaceVisitor(expression.Parameters[0], parameter).Visit(expression.Body)!;
    }

    private sealed class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _from ? _to : base.VisitParameter(node);
        }
    }

    private LambdaExpression? BuildSoftDeleteFilterExpression(Type entityType)
    {
        if (!typeof(ISoftDelete).IsAssignableFrom(entityType))
        {
            return null;
        }

        var parameter = Expression.Parameter(entityType, "e");
        var isDeletedProperty = Expression.Property(
            Expression.Convert(parameter, typeof(ISoftDelete)),
            nameof(ISoftDelete.IsDeleted));

        var notDeleted = Expression.Not(isDeletedProperty);

        return ApplySoftDeleteFiltersToggle(Expression.Lambda(notDeleted, parameter));
    }

    private LambdaExpression? BuildTenantFilterExpression(Type entityType)
    {
        var currentTenantId = Expression.Property(Expression.Constant(this), nameof(SessionTenantId));
        var tenantIsNull = Expression.Equal(currentTenantId, Expression.Constant(null, typeof(Guid?)));

        if (typeof(IMustHaveTenant).IsAssignableFrom(entityType))
        {
            var parameter = Expression.Parameter(entityType, "e");
            var tenantProperty = Expression.Property(
                Expression.Convert(parameter, typeof(IMustHaveTenant)),
                nameof(IMustHaveTenant.TenantId));

            var tenantMatches = Expression.Equal(
                Expression.Convert(tenantProperty, typeof(Guid?)),
                currentTenantId);

            var tenantFilter = Expression.OrElse(tenantIsNull, tenantMatches);

            return ApplyTenantFiltersToggle(Expression.Lambda(tenantFilter, parameter));
        }

        if (typeof(IMayHaveTenant).IsAssignableFrom(entityType))
        {
            var parameter = Expression.Parameter(entityType, "e");
            var tenantProperty = Expression.Property(
                Expression.Convert(parameter, typeof(IMayHaveTenant)),
                nameof(IMayHaveTenant.TenantId));

            var tenantIsNullOnEntity = Expression.Equal(
                tenantProperty,
                Expression.Constant(null, typeof(Guid?)));

            var tenantMatches = Expression.Equal(tenantProperty, currentTenantId);
            var tenantFilter = Expression.OrElse(
                tenantIsNull,
                Expression.OrElse(tenantIsNullOnEntity, tenantMatches));

            return ApplyTenantFiltersToggle(Expression.Lambda(tenantFilter, parameter));
        }

        return null;
    }

    private LambdaExpression ApplySoftDeleteFiltersToggle(LambdaExpression filter)
    {
        var softDeleteFiltersEnabled = Expression.Property(
            Expression.Constant(this),
            nameof(SoftDeleteFiltersEnabled));

        var ignoreFilters = Expression.Not(softDeleteFiltersEnabled);
        var finalFilter = Expression.OrElse(ignoreFilters, filter.Body);

        return Expression.Lambda(finalFilter, filter.Parameters);
    }

    private LambdaExpression ApplyTenantFiltersToggle(LambdaExpression filter)
    {
        var tenantFiltersEnabled = Expression.Property(
            Expression.Constant(this),
            nameof(TenantFiltersEnabled));

        var ignoreFilters = Expression.Not(tenantFiltersEnabled);
        var finalFilter = Expression.OrElse(ignoreFilters, filter.Body);

        return Expression.Lambda(finalFilter, filter.Parameters);
    }

    private bool ResolveTenantFiltersEnabled()
    {
        var endpoint = _httpContextAccessor.HttpContext?.GetEndpoint();
        return endpoint?.Metadata.GetMetadata<SkipTenantAccessValidationAttribute>() is null;
    }

    private void ApplySoftDeleteRules()
    {
        var userId = ResolveUserId();
        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.Entity.IsDeleted = true;
            entry.State = EntityState.Modified;
            entry.Property(nameof(ISoftDelete.IsDeleted)).IsModified = true;

            if (entry.Entity is AuditableEntity auditableEntity)
            {
                auditableEntity.DeletedDate = DateTime.UtcNow;
                auditableEntity.DeletedBy = userId;
            }
        }
    }

    private void ApplyAuditRules()
    {
        var now = DateTime.UtcNow;
        var userId = ResolveUserId();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate ??= now;
                entry.Entity.UpdatedDate = now;

                if (string.IsNullOrWhiteSpace(entry.Entity.CreatedBy))
                {
                    entry.Entity.CreatedBy = userId;
                }

                entry.Entity.UpdatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(AuditableEntity.CreatedDate)).IsModified = false;
                entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;

                entry.Entity.UpdatedDate = now;
                entry.Entity.UpdatedBy = userId;
            }
        }
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

    private static void ApplyTenantIndexes(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(IMustHaveTenant.TenantId))
                    .HasDatabaseName($"IX_{entityType.ClrType.Name}_TenantId");
                continue;
            }

            if (typeof(IMayHaveTenant).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(IMayHaveTenant.TenantId))
                    .HasDatabaseName($"IX_{entityType.ClrType.Name}_TenantId");
            }
        }
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
