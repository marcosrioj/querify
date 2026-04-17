using BaseFaq.Models.QnA.Dtos.Source;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Source.Commands.CreateSource;

public sealed class SourcesCreateSourceCommand : IRequest<Guid>
{
    public required SourceCreateRequestDto Request { get; set; }
}