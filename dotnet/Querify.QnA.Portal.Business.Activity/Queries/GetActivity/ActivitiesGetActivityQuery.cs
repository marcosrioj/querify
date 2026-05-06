using Querify.Models.QnA.Dtos.Activity;
using MediatR;

namespace Querify.QnA.Portal.Business.Activity.Queries.GetActivity;

public sealed class ActivitiesGetActivityQuery : IRequest<ActivityDto>
{
    public required Guid Id { get; set; }
}