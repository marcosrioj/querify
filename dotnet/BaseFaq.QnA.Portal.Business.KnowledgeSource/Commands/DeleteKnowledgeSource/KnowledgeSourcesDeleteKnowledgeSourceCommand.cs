using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands;

public sealed class KnowledgeSourcesDeleteKnowledgeSourceCommand : IRequest
{
    public Guid Id { get; set; }
}
