using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Activity;
using Querify.QnA.Portal.Business.Activity.Abstractions;
using Querify.QnA.Portal.Business.Activity.Queries.GetActivity;
using Querify.QnA.Portal.Business.Activity.Queries.GetActivityList;
using MediatR;

namespace Querify.QnA.Portal.Business.Activity.Service;

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