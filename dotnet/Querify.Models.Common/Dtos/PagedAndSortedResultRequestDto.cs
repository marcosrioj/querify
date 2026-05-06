namespace Querify.Models.Common.Dtos;

[Serializable]
public class PagedAndSortedResultRequestDto : PagedResultRequestDto
{
    /// <summary>
    /// Sorting information.
    /// Should include sorting field and optionally a direction (ASC or DESC)
    /// Can contain more than one field separated by comma (,).
    /// </summary>
    /// <example>
    /// Examples:
    /// "Name"
    /// "Name DESC"
    /// "Name ASC, Age DESC"
    /// </example>
    public string? Sorting { get; set; }
}