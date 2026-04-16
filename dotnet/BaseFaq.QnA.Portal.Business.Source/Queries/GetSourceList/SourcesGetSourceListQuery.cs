using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Source;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Source.Queries.GetSourceList;

public sealed class SourcesGetSourceListQuery : IRequest<PagedResultDto<SourceDto>>
{
    public required SourceGetAllRequestDto Request { get; set; }
}