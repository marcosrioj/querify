namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Defines the product surface primarily using or producing a Q&amp;A asset.
/// </summary>
public enum QnAProductSurface
{
    /// <summary>
    /// Canonical answer publishing for owned websites, docs, portals, and embeds.
    /// </summary>
    Publish = 1,

    /// <summary>
    /// Guided resolution experiences powered by approved answers.
    /// </summary>
    Resolve = 2,

    /// <summary>
    /// Public, social, messaging, or community signal capture.
    /// </summary>
    Listen = 3,

    /// <summary>
    /// Moderated community support, ideas, roadmap, and peer contribution.
    /// </summary>
    Collaborate = 4,

    /// <summary>
    /// Transparent, auditable, or publicly verifiable decisions.
    /// </summary>
    Govern = 5
}
