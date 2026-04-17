using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Activity;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Activity.Queries.GetActivityList;

public sealed class ActivitiesGetActivityListQuery : IRequest<PagedResultDto<ActivityDto>>
{
    public required ActivityGetAllRequestDto Request { get; set; }
}