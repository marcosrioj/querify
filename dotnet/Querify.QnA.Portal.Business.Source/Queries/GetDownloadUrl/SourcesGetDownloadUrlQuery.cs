using Querify.Models.QnA.Dtos.Source;
using MediatR;

namespace Querify.QnA.Portal.Business.Source.Queries.GetDownloadUrl;

public sealed class SourcesGetDownloadUrlQuery : IRequest<SourceDownloadUrlDto>
{
    public required Guid Id { get; set; }
}
