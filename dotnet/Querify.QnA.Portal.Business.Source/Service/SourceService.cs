using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Source;
using Querify.QnA.Portal.Business.Source.Abstractions;
using Querify.QnA.Portal.Business.Source.Commands.CreateSource;
using Querify.QnA.Portal.Business.Source.Commands.DeleteSource;
using Querify.QnA.Portal.Business.Source.Commands.UpdateSource;
using Querify.QnA.Portal.Business.Source.Queries.GetSource;
using Querify.QnA.Portal.Business.Source.Queries.GetSourceList;
using MediatR;

namespace Querify.QnA.Portal.Business.Source.Service;

public sealed class SourceService(IMediator mediator) : ISourceService
{
    public Task<Guid> Create(SourceCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new SourcesCreateSourceCommand { Request = dto }, token);
    }

    public Task<PagedResultDto<SourceDto>> GetAll(SourceGetAllRequestDto requestDto,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new SourcesGetSourceListQuery { Request = requestDto }, token);
    }

    public Task<SourceDetailDto> GetById(Guid id, CancellationToken token)
    {
        return mediator.Send(new SourcesGetSourceQuery { Id = id }, token);
    }

    public Task<Guid> Update(Guid id, SourceUpdateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new SourcesUpdateSourceCommand { Id = id, Request = dto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new SourcesDeleteSourceCommand { Id = id }, token);
    }
}
