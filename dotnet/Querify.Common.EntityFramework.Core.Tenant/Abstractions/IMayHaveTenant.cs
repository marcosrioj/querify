namespace Querify.Common.EntityFramework.Core.Abstractions;

public interface IMayHaveTenant
{
    Guid? TenantId { get; }
}
