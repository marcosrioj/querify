using System.Linq.Expressions;
using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Core.SoftDelete.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Querify.Common.EntityFramework.Core.SoftDelete.DbContext.SoftDelete;

public static class SoftDeleteQueryFilterExtension
{
    public static void ApplySoftDeleteFilters(
        this ModelBuilder modelBuilder,
        ISoftDeleteFilterState filterState)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var filter = BuildSoftDeleteFilterExpression(entityType.ClrType, filterState);
            if (filter is null)
            {
                continue;
            }

            ApplyQueryFilter(modelBuilder, entityType.ClrType, filter);
        }
    }

    private static LambdaExpression? BuildSoftDeleteFilterExpression(
        Type entityType,
        ISoftDeleteFilterState filterState)
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
        return ApplySoftDeleteFiltersToggle(Expression.Lambda(notDeleted, parameter), filterState);
    }

    private static LambdaExpression ApplySoftDeleteFiltersToggle(
        LambdaExpression filter,
        ISoftDeleteFilterState filterState)
    {
        var softDeleteFiltersEnabled = Expression.Property(
            Expression.Constant(filterState),
            nameof(ISoftDeleteFilterState.SoftDeleteFiltersEnabled));

        var ignoreFilters = Expression.Not(softDeleteFiltersEnabled);
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
