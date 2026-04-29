using BaseFaq.Models.QnA.Dtos.Activity;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Mappings;

public static class ActivityMapping
{
    public static ActivityDto ToActivityDto(this Activity entity)
    {
        return new ActivityDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            QuestionId = entity.QuestionId,
            AnswerId = entity.AnswerId,
            Kind = entity.Kind,
            ActorKind = entity.ActorKind,
            ActorLabel = entity.ActorLabel,
            UserPrint = entity.UserPrint,
            Ip = entity.Ip,
            UserAgent = entity.UserAgent,
            Notes = entity.Notes,
            MetadataJson = entity.MetadataJson,
            OccurredAtUtc = entity.OccurredAtUtc
        };
    }
}
