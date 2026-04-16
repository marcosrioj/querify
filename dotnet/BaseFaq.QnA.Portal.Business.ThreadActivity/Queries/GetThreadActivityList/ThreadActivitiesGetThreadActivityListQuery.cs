using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.ThreadActivity;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.ThreadActivity.Queries.GetThreadActivityList;

public sealed class ThreadActivitiesGetThreadActivityListQuery : IRequest<PagedResultDto<ThreadActivityDto>>
{
    public required ThreadActivityGetAllRequestDto Request { get; set; }
}