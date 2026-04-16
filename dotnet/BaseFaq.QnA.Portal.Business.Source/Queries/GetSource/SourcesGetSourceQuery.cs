using BaseFaq.Models.QnA.Dtos.Source;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Source.Queries.GetSource;

public sealed class SourcesGetSourceQuery : IRequest<SourceDto>
{
    public Guid Id { get; set; }
}