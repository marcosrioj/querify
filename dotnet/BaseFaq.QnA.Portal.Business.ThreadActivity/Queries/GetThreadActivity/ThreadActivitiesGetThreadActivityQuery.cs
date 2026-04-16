using BaseFaq.Models.QnA.Dtos.ThreadActivity;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.ThreadActivity.Queries.GetThreadActivity;

public sealed class ThreadActivitiesGetThreadActivityQuery : IRequest<ThreadActivityDto>
{
    public Guid Id { get; set; }
}