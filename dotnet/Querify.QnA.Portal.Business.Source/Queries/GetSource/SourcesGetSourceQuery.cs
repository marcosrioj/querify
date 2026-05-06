using Querify.Models.QnA.Dtos.Source;
using MediatR;

namespace Querify.QnA.Portal.Business.Source.Queries.GetSource;

public sealed class SourcesGetSourceQuery : IRequest<SourceDetailDto>
{
    public required Guid Id { get; set; }
}
