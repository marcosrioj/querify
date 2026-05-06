using System.Linq.Expressions;
using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Core.Tenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Querify.Common.EntityFramework.Core.Tenant.DbContext.Tenant;

public static class TenantModelConfigurationExtension
{
    public static void ApplyTenantFilters(
        this ModelBuilder modelBuilder,
        ITenantFilterState filterState)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var filter = BuildTenantFilterExpression(entityType.ClrType, filterState);
            if (filter is null)
            {
                continue;
            }

            ApplyQueryFilter(modelBuilder, entityType.ClrType, filter);
        }
    }

    public static void ApplyTenantIndexes(this ModelBuilder modelBuilder)
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

    private static LambdaExpression? BuildTenantFilterExpression(
        Type entityType,
        ITenantFilterState filterState)
    {
        var filterStateExpression = Expression.Constant(filterState, typeof(ITenantFilterState));
        var currentTenantId = Expression.Property(
            filterStateExpression,
            nameof(ITenantFilterState.SessionTenantId));
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

            return ApplyTenantFiltersToggle(Expression.Lambda(tenantFilter, parameter), filterState);
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

            return ApplyTenantFiltersToggle(Expression.Lambda(tenantFilter, parameter), filterState);
        }

        return null;
    }

    private static LambdaExpression ApplyTenantFiltersToggle(
        LambdaExpression filter,
        ITenantFilterState filterState)
    {
        var tenantFiltersEnabled = Expression.Property(
            Expression.Constant(filterState, typeof(ITenantFilterState)),
            nameof(ITenantFilterState.TenantFiltersEnabled));

        var ignoreFilters = Expression.Not(tenantFiltersEnabled);
        var finalFilter = Expression.OrElse(ignoreFilters, filter.Body);

        return Expression.Lambda(finalFilter, filter.Parameters);
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
}
