using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Topic;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Topic.Commands;

public sealed class TopicsCreateTopicCommand : IRequest<Guid>
{
    public required TopicCreateRequestDto Request { get; set; }
}
