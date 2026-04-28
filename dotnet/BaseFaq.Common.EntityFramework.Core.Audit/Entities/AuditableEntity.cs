using BaseFaq.Common.EntityFramework.Core.Abstractions;

namespace BaseFaq.Common.EntityFramework.Core.Entities;

public class AuditableEntity : ISoftDelete
{
    public DateTime? CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
}
