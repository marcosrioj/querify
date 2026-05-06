namespace Querify.Common.EntityFramework.Core.Abstractions;

public interface IMustHaveTenant
{
    Guid TenantId { get; }
}
