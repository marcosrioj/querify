using System.ComponentModel.DataAnnotations;

namespace Querify.Common.Infrastructure.Storage.Options;

/// <summary>
///     Configures S3-compatible private object storage for tenant-prefixed uploaded objects.
/// </summary>
public sealed class ObjectStorageOptions
{
    /// <summary>
    ///     Configuration section name used by API and worker hosts.
    /// </summary>
    public const string SectionName = "ObjectStorage";

    /// <summary>
    ///     Backend-reachable S3-compatible service endpoint used for storage operations.
    /// </summary>
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    ///     Browser-reachable endpoint used only when generating presigned URLs.
    /// </summary>
    public string? PublicEndpoint { get; set; }

    /// <summary>
    ///     Signing region for the S3-compatible provider.
    /// </summary>
    [Required]
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    ///     Access key for the private bucket credentials.
    /// </summary>
    [Required]
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    ///     Secret key for the private bucket credentials.
    /// </summary>
    [Required]
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    ///     Private bucket that stores staging, verified, and quarantine objects.
    /// </summary>
    [Required]
    public string Bucket { get; set; } = string.Empty;

    /// <summary>
    ///     Uses path-style addressing for S3-compatible providers such as MinIO.
    /// </summary>
    public bool ForcePathStyle { get; set; } = true;

    /// <summary>
    ///     Default lifetime for presigned PUT URLs issued by upload intent handlers.
    /// </summary>
    [Range(1, 1440)]
    public int UploadPresignTtlMinutes { get; set; } = 10;

    /// <summary>
    ///     Default lifetime for private presigned GET URLs issued for verified objects.
    /// </summary>
    [Range(1, 1440)]
    public int DownloadPresignTtlMinutes { get; set; } = 5;

    /// <summary>
    ///     Optional S3 server-side encryption mode that must be signed and sent with PUT requests.
    /// </summary>
    public string? ServerSideEncryptionMode { get; set; }
}
