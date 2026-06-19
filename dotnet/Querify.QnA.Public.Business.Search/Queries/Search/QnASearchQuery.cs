using MediatR;
using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Search;

namespace Querify.QnA.Public.Business.Search.Queries.Search;

public sealed class QnASearchQuery : IRequest<PagedResultDto<QnASearchResultDto>>
{
    public required QnASearchRequestDto Request { get; set; }
}
