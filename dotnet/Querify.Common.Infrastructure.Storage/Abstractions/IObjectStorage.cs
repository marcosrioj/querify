namespace Querify.Common.Infrastructure.Storage.Abstractions;

/// <summary>
///     Provides private object storage operations and short-lived single-object presigned URLs.
/// </summary>
public interface IObjectStorage
{
    /// <summary>
    ///     Creates a short-lived presigned PUT URL for one object key and declared content type.
    /// </summary>
    Task<PresignedPutResult> PresignPutAsync(
        string key,
        string contentType,
        long expectedSizeBytes,
        CancellationToken ct);

    /// <summary>
    ///     Creates a short-lived private presigned GET URL for one object key.
    /// </summary>
    Task<Uri> PresignGetAsync(string key, TimeSpan ttl, CancellationToken ct);

    /// <summary>
    ///     Reads object metadata without loading object bytes.
    /// </summary>
    Task<ObjectMetadata?> HeadAsync(string key, CancellationToken ct);

    /// <summary>
    ///     Copies an object inside the configured private bucket.
    /// </summary>
    Task CopyAsync(string sourceKey, string destinationKey, CancellationToken ct);

    /// <summary>
    ///     Deletes an object from the configured private bucket.
    /// </summary>
    Task DeleteAsync(string key, CancellationToken ct);

    /// <summary>
    ///     Opens an object stream for reading without buffering the whole object in memory.
    /// </summary>
    Task<Stream> OpenReadAsync(string key, CancellationToken ct);
}

/// <summary>
///     Result returned when a caller receives a presigned PUT URL and the exact headers it must send.
/// </summary>
public sealed record PresignedPutResult(
    Uri Url,
    IReadOnlyDictionary<string, string> RequiredHeaders,
    DateTime ExpiresAtUtc);

/// <summary>
///     Object metadata returned by storage HEAD operations.
/// </summary>
public sealed record ObjectMetadata(
    long SizeBytes,
    string ContentType,
    string ETag,
    DateTime LastModifiedUtc);
