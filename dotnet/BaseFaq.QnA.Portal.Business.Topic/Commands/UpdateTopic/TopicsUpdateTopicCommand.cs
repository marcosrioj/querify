using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.Topic;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.Topic.Commands.UpdateTopic;

public sealed class TopicsUpdateTopicCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public required TopicUpdateRequestDto Request { get; set; }
}
