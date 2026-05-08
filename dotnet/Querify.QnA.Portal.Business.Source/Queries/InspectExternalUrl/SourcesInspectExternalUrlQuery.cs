using Querify.Models.QnA.Dtos.Source;
using MediatR;

namespace Querify.QnA.Portal.Business.Source.Queries.InspectExternalUrl;

public sealed class SourcesInspectExternalUrlQuery : IRequest<SourceExternalUrlInspectionDto>
{
    public required SourceExternalUrlInspectionRequestDto Request { get; set; }
}
