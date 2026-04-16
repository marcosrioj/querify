using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Activity;
using BaseFaq.QnA.Portal.Business.Activity.Abstractions;
using BaseFaq.QnA.Portal.Business.Activity.Queries.GetActivity;
using BaseFaq.QnA.Portal.Business.Activity.Queries.GetActivityList;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Activity.Service;

public sealed class ActivityService(IMediator mediator) : IActivityService
{
    public Task<PagedResultDto<ActivityDto>> GetAll(ActivityGetAllRequestDto requestDto,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new ActivitiesGetActivityListQuery { Request = requestDto }, token);
    }

    public Task<ActivityDto> GetById(Guid id, CancellationToken token)
    {
        return mediator.Send(new ActivitiesGetActivityQuery { Id = id }, token);
    }
}