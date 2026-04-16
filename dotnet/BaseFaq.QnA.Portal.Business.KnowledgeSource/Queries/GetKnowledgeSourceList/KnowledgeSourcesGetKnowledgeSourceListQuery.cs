using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Queries;

public sealed class KnowledgeSourcesGetKnowledgeSourceListQuery : IRequest<PagedResultDto<KnowledgeSourceDto>>
{
    public required KnowledgeSourceGetAllRequestDto Request { get; set; }
}
