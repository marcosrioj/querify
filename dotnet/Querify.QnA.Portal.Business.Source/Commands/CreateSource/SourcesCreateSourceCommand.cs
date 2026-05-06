using Querify.Models.QnA.Dtos.Source;
using MediatR;

namespace Querify.QnA.Portal.Business.Source.Commands.CreateSource;

public sealed class SourcesCreateSourceCommand : IRequest<Guid>
{
    public required SourceCreateRequestDto Request { get; set; }
}