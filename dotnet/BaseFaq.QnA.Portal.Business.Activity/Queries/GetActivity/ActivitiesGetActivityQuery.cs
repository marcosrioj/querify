using BaseFaq.Models.QnA.Dtos.Activity;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Activity.Queries.GetActivity;

public sealed class ActivitiesGetActivityQuery : IRequest<ActivityDto>
{
    public Guid Id { get; set; }
}