using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Abstractions;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.CreateKnowledgeSource;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.DeleteKnowledgeSource;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.UpdateKnowledgeSource;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Queries.GetKnowledgeSource;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Queries.GetKnowledgeSourceList;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Service;

public sealed class KnowledgeSourceService(IMediator mediator) : IKnowledgeSourceService
{
    public Task<Guid> Create(KnowledgeSourceCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new KnowledgeSourcesCreateKnowledgeSourceCommand { Request = dto }, token);
    }

    public Task<PagedResultDto<KnowledgeSourceDto>> GetAll(KnowledgeSourceGetAllRequestDto requestDto,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new KnowledgeSourcesGetKnowledgeSourceListQuery { Request = requestDto }, token);
    }

    public Task<KnowledgeSourceDto> GetById(Guid id, CancellationToken token)
    {
        return mediator.Send(new KnowledgeSourcesGetKnowledgeSourceQuery { Id = id }, token);
    }

    public Task<Guid> Update(Guid id, KnowledgeSourceUpdateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new KnowledgeSourcesUpdateKnowledgeSourceCommand { Id = id, Request = dto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new KnowledgeSourcesDeleteKnowledgeSourceCommand { Id = id }, token);
    }
}