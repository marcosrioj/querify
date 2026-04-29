using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Mappings;

public static class SourceMapping
{
    public static SourceDto ToSourceDto(this Source entity)
    {
        return new SourceDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Kind = entity.Kind,
            Locator = entity.Locator,
            Label = entity.Label,
            ContextNote = entity.ContextNote,
            ExternalId = entity.ExternalId,
            Language = entity.Language,
            MediaType = entity.MediaType,
            Checksum = entity.Checksum,
            MetadataJson = entity.MetadataJson,
            Visibility = entity.Visibility,
            LastVerifiedAtUtc = entity.LastVerifiedAtUtc,
            SpaceUsageCount = entity.Spaces.Count,
            QuestionUsageCount = entity.Questions.Count,
            AnswerUsageCount = entity.Answers.Count
        };
    }

    public static QuestionSourceLinkDto ToQuestionSourceLinkDto(this QuestionSourceLink entity)
    {
        return new QuestionSourceLinkDto
        {
            Id = entity.Id,
            QuestionId = entity.QuestionId,
            SourceId = entity.SourceId,
            Role = entity.Role,
            Order = entity.Order,
            Source = entity.Source?.ToSourceDto()
        };
    }

    public static AnswerSourceLinkDto ToAnswerSourceLinkDto(this AnswerSourceLink entity)
    {
        return new AnswerSourceLinkDto
        {
            Id = entity.Id,
            AnswerId = entity.AnswerId,
            SourceId = entity.SourceId,
            Role = entity.Role,
            Order = entity.Order,
            Source = entity.Source?.ToSourceDto()
        };
    }
}
