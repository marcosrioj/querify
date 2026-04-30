using BaseFaq.Models.QnA.Dtos.Tag;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Mappings;

public static class TagMapping
{
    public static TagDto ToTagDto(this Tag entity)
    {
        return new TagDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            SpaceUsageCount = entity.Spaces.Count,
            QuestionUsageCount = entity.Questions.Count,
            LastUpdatedAtUtc = entity.UpdatedDate ?? entity.CreatedDate
        };
    }
}
