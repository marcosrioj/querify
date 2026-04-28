namespace BaseFaq.Common.EntityFramework.Core.SoftDelete.Abstractions;

public interface ISoftDeleteFilterState
{
    bool SoftDeleteFiltersEnabled { get; }
}
