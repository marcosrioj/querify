using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Source;
using MediatR;

namespace Querify.QnA.Portal.Business.Source.Queries.GetSourceList;

public sealed class SourcesGetSourceListQuery : IRequest<PagedResultDto<SourceDto>>
{
    public required SourceGetAllRequestDto Request { get; set; }
}