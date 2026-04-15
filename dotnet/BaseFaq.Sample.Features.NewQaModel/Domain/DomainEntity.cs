namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public abstract class DomainEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    public DateTime? ArchivedAtUtc { get; set; }
    public string? ArchivedBy { get; set; }

    public bool IsArchived { get; set; }
}
