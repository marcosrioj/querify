using Querify.Models.QnA.Dtos.Source;
using MediatR;

namespace Querify.QnA.Portal.Business.Source.Commands.CreateUploadIntent;

public sealed class SourcesCreateUploadIntentCommand : IRequest<SourceUploadIntentResponseDto>
{
    public required SourceUploadIntentRequestDto Dto { get; set; }
}
