using System.Text.Json;

namespace BaseFaq.Sample.Features.NewQaModel.Domain;

internal static class DomainGuards
{
    public static Guid TenantIdOf(DomainEntity? entity, string paramName)
    {
        ArgumentNullException.ThrowIfNull(entity, paramName);
        return entity.TenantId;
    }

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
