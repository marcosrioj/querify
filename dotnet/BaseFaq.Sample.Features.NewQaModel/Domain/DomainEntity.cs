namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public abstract class DomainEntity
{
    private const int MaxActorLength = 200;

    protected DomainEntity(Guid tenantId, string? createdBy = null, DateTime? createdAtUtc = null)
    {
        var createdAt = DomainGuards.Utc(createdAtUtc ?? DateTime.UtcNow, nameof(createdAtUtc));

        Id = Guid.NewGuid();
        TenantId = DomainGuards.AgainstEmpty(tenantId, nameof(tenantId));
        CreatedAtUtc = createdAt;
        CreatedBy = DomainGuards.Optional(createdBy, MaxActorLength, nameof(createdBy));
        UpdatedAtUtc = createdAt;
        UpdatedBy = CreatedBy;
    }

    protected DomainEntity()
    {
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }
    public string? UpdatedBy { get; private set; }

    public DateTime? ArchivedAtUtc { get; private set; }
    public string? ArchivedBy { get; private set; }

    public bool IsArchived { get; private set; }

    public virtual void Archive(string? archivedBy = null, DateTime? archivedAtUtc = null)
    {
        if (IsArchived)
        {
            return;
        }

        var archivedAt = DomainGuards.Utc(archivedAtUtc ?? DateTime.UtcNow, nameof(archivedAtUtc));

        IsArchived = true;
        ArchivedAtUtc = archivedAt;
        ArchivedBy = DomainGuards.Optional(archivedBy, MaxActorLength, nameof(archivedBy));
        Touch(ArchivedBy, archivedAt);
    }

    protected void Touch(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        UpdatedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        UpdatedBy = DomainGuards.Optional(updatedBy, MaxActorLength, nameof(updatedBy));
    }

    protected void EnsureSameTenant(DomainEntity other, string relationshipName)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameTenant(this, other, relationshipName);
    }

    protected static void EnsureSameTenant(DomainEntity left, DomainEntity right, string relationshipName)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        DomainGuards.Ensure(
            left.TenantId == right.TenantId,
            $"Cross-tenant relationship is not allowed for {relationshipName}.");
    }
}
