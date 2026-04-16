namespace BaseFaq.Models.QnA.Dtos.Space;

public class SpaceDetailDto : SpaceDto
{
    public IReadOnlyList<BaseFaq.Models.QnA.Dtos.Tag.TagDto> Tags { get; set; } = [];
    public IReadOnlyList<BaseFaq.Models.QnA.Dtos.Source.SourceDto> CuratedSources { get; set; } = [];
}
