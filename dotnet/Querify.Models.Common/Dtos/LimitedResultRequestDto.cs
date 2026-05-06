using System.ComponentModel.DataAnnotations;

namespace Querify.Models.Common.Dtos;

public class LimitedResultRequestDto
{
    public static int DefaultMaxResultCount { get; set; } = 10;

    [Range(1, int.MaxValue)] public virtual int MaxResultCount { get; set; } = DefaultMaxResultCount;
}