using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.Common.Enums;

namespace BaseFaq.Common.EntityFramework.Tenant.Entities;

public class TenantConnection : BaseEntity
{
    public const int MaxConnectionStringLength = 1024;

    public required string ConnectionString { get; set; }

    /// <summary>
    /// BaseFaq module that uses this current connection string for tenant module databases.
    /// </summary>
    public required ModuleEnum Module { get; set; }

    public required bool IsCurrent { get; set; }
}
