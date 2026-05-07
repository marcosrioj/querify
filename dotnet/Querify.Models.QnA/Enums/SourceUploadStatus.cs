namespace Querify.Models.QnA.Enums;

/// <summary>
/// Describes the trusted processing state of an uploaded source file.
/// </summary>
public enum SourceUploadStatus
{
    /// <summary>
    /// The source points to an external locator and has no upload workflow.
    /// </summary>
    None = 1,

    /// <summary>
    /// An upload intent exists, but the client has not finalized the object write.
    /// </summary>
    Pending = 6,

    /// <summary>
    /// The client confirmed upload completion and asynchronous verification is pending.
    /// </summary>
    Uploaded = 11,

    /// <summary>
    /// The worker validated checksum, size, type, and scan requirements.
    /// </summary>
    Verified = 16,

    /// <summary>
    /// The worker retained an unsafe artifact for security triage and blocked downloads.
    /// </summary>
    Quarantined = 21,

    /// <summary>
    /// The upload was permanently rejected and no downloadable artifact remains.
    /// </summary>
    Failed = 26,

    /// <summary>
    /// The upload intent aged out before completion and staging bytes were removed or absent.
    /// </summary>
    Expired = 31
}
