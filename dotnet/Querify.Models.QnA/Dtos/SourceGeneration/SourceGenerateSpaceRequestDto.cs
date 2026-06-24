namespace Querify.Models.QnA.Dtos.SourceGeneration;

public sealed class SourceGenerateSpaceRequestDto
{
    /// <summary>
    ///     Optional goal or audience note that guides automatic source-to-space planning.
    /// </summary>
    public string? ExtractionGoal { get; set; }

    /// <summary>
    ///     Optional section, range, or topic hint inside large source material.
    /// </summary>
    public string? ContentHint { get; set; }
}
