using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using System.Text.Json;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Guards;

/// <summary>
/// Centralizes normalization and invariant checks used by the Q&amp;A persistence domain model.
/// </summary>
public static class DomainGuards
{
    private const int MaxActorLength = 200;

    public static Guid AgainstEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Value cannot be an empty GUID.", paramName);
        }

        return value;
    }

    public static string Required(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", paramName);
        }

        var trimmedValue = value.Trim();
        EnsureMaxLength(trimmedValue, maxLength, paramName);
        return trimmedValue;
    }

    public static string? Optional(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();
        EnsureMaxLength(trimmedValue, maxLength, paramName);
        return trimmedValue;
    }

    public static string? Json(string? value, int maxLength, string paramName)
    {
        var normalizedValue = Optional(value, maxLength, paramName);
        if (normalizedValue is null)
        {
            return null;
        }

        try
        {
            using var _ = JsonDocument.Parse(normalizedValue);
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("Value must be valid JSON.", paramName, exception);
        }

        return normalizedValue;
    }

    public static string? UriString(string? value, int maxLength, string paramName)
    {
        var normalizedValue = Optional(value, maxLength, paramName);
        if (normalizedValue is null)
        {
            return null;
        }

        if (!Uri.TryCreate(normalizedValue, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(uri.Scheme))
        {
            throw new ArgumentException("Value must be a valid absolute URI.", paramName);
        }

        return normalizedValue;
    }

    public static int Range(int value, int minInclusive, int maxInclusive, string paramName)
    {
        if (value < minInclusive || value > maxInclusive)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"Value must be between {minInclusive} and {maxInclusive}.");
        }

        return value;
    }

    public static int NonNegative(int value, string paramName) => Range(value, 0, int.MaxValue, paramName);

    public static DateTime Utc(DateTime value, string paramName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Value must use DateTimeKind.Utc.", paramName);
        }

        return value;
    }

    public static void InitializeAudit(BaseEntity entity, string? createdBy = null, DateTime? createdAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.CreatedBy = Optional(createdBy, MaxActorLength, nameof(createdBy));
        entity.UpdatedBy = entity.CreatedBy;

        if (createdAtUtc is not DateTime createdAt)
        {
            return;
        }

        entity.CreatedDate = Utc(createdAt, nameof(createdAtUtc));
        entity.UpdatedDate = entity.CreatedDate;
    }

    public static void Touch(BaseEntity entity, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.UpdatedDate = Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        entity.UpdatedBy = Optional(updatedBy, MaxActorLength, nameof(updatedBy));
    }

    public static Guid TenantIdOf<TEntity>(TEntity? entity, string paramName)
        where TEntity : class, IMustHaveTenant
    {
        ArgumentNullException.ThrowIfNull(entity, paramName);
        return entity.TenantId;
    }

    public static void EnsureSameTenant<TLeft, TRight>(TLeft left, TRight right, string relationshipName)
        where TLeft : class, IMustHaveTenant
        where TRight : class, IMustHaveTenant
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        Ensure(
            left.TenantId == right.TenantId,
            $"Cross-tenant relationship is not allowed for {relationshipName}.");
    }

    public static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void EnsureMaxLength(string value, int maxLength, string paramName)
    {
        if (value.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", paramName);
        }
    }
}
