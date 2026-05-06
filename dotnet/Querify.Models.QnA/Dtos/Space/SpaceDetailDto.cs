namespace Querify.Models.QnA.Dtos.Space;

public class SpaceDetailDto : SpaceDto
{
    public IReadOnlyList<Querify.Models.QnA.Dtos.Tag.TagDto> Tags { get; set; } = [];
    public IReadOnlyList<Querify.Models.QnA.Dtos.Source.SourceDto> CuratedSources { get; set; } = [];
}
