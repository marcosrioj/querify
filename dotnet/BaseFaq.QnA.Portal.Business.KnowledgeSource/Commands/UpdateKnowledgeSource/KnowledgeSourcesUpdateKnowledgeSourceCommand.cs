using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.UpdateKnowledgeSource;

public sealed class KnowledgeSourcesUpdateKnowledgeSourceCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public required KnowledgeSourceUpdateRequestDto Request { get; set; }
}
