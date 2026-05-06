using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Activity;
using MediatR;

namespace Querify.QnA.Portal.Business.Activity.Queries.GetActivityList;

public sealed class ActivitiesGetActivityListQuery : IRequest<PagedResultDto<ActivityDto>>
{
    public required ActivityGetAllRequestDto Request { get; set; }
}