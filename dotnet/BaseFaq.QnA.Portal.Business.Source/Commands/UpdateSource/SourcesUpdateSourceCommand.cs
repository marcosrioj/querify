using BaseFaq.Models.QnA.Dtos.Source;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Source.Commands.UpdateSource;

public sealed class SourcesUpdateSourceCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
    public required SourceUpdateRequestDto Request { get; set; }
}