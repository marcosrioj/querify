using System.ComponentModel.DataAnnotations;

namespace Querify.Models.Common.Dtos;

[Serializable]
public class PagedResultRequestDto : LimitedResultRequestDto
{
    /// <summary>
    /// Skip count (beginning of the page).
    /// </summary>
    [Range(0, int.MaxValue)]
    public virtual int SkipCount { get; set; }
}