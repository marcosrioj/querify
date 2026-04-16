using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.ThreadActivity;
using BaseFaq.QnA.Portal.Business.ThreadActivity.Abstractions;
using BaseFaq.QnA.Portal.Business.ThreadActivity.Queries.GetThreadActivity;
using BaseFaq.QnA.Portal.Business.ThreadActivity.Queries.GetThreadActivityList;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.ThreadActivity.Service;

public sealed class ThreadActivityService(IMediator mediator) : IThreadActivityService
{
    public Task<PagedResultDto<ThreadActivityDto>> GetAll(ThreadActivityGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new ThreadActivitiesGetThreadActivityListQuery { Request = requestDto }, token);
    }

    public Task<ThreadActivityDto> GetById(Guid id, CancellationToken token)
    {
        return mediator.Send(new ThreadActivitiesGetThreadActivityQuery { Id = id }, token);
    }
}
