namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Defines the operating model of a question space.
/// This controls whether the space behaves as a curated knowledge surface,
/// a community-driven area, or a mix of both.
/// </summary>
public enum SpaceKind
{
    /// <summary>
    /// Questions and answers are mostly curated by the product, support, or editorial team.
    /// Community input may exist later, but it is not the default operating assumption.
    /// </summary>
    CuratedKnowledge = 1,

    /// <summary>
    /// The space is primarily open to community participation.
    /// Ranking, moderation, and answer acceptance usually matter more than editorial ordering.
    /// </summary>
    Community = 2,

    /// <summary>
    /// The space combines official knowledge with community participation.
    /// This is the most flexible mode for products that need both trusted answers and open discussion.
    /// </summary>
    Hybrid = 3
}
