using BaseFaq.Models.QnA.Dtos.Space;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Mappings;

public static class SpaceMapping
{
    public static SpaceDto ToSpaceDto(this Space entity)
    {
        return new SpaceDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Slug = entity.Slug,
            Summary = entity.Summary,
            Language = entity.Language,
            Status = entity.Status,
            Visibility = entity.Visibility,
            AcceptsQuestions = entity.AcceptsQuestions,
            AcceptsAnswers = entity.AcceptsAnswers,
            QuestionCount = entity.Questions.Count,
            LastUpdatedAtUtc = entity.UpdatedDate ?? entity.CreatedDate
        };
    }
}
