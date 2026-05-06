using Querify.Common.EntityFramework.Core.Entities;
using Querify.Models.Common.Enums;
using Querify.Models.Tenant.Enums;

namespace Querify.Common.EntityFramework.Tenant.Entities;

public class Tenant : BaseEntity
{
    public const int MaxSlugLength = 128;
    public const int MaxNameLength = 128;
    public const int MaxConnectionStringLength = 1024;
    public const int MaxClientKeyLength = 128;
    public const string DefaultTenantName = "Default";

    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required TenantEdition Edition { get; set; }

    /// <summary>
    /// Querify module that owns this tenant runtime record and its module database connection.
    /// </summary>
    public required ModuleEnum Module { get; set; }

    public required string ConnectionString { get; set; }
    public string? ClientKey { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<TenantUser> TenantUsers { get; set; } = [];
}
