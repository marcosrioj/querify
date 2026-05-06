namespace Querify.Common.EntityFramework.Core.Abstractions;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
