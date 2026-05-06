namespace Querify.Common.EntityFramework.Core.SoftDelete.Abstractions;

public interface ISoftDeleteFilterState
{
    bool SoftDeleteFiltersEnabled { get; }
}
