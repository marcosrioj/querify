namespace BaseFaq.Common.EntityFramework.Core.Abstractions;

public interface IMayHaveTenant
{
    Guid? TenantId { get; }
}
