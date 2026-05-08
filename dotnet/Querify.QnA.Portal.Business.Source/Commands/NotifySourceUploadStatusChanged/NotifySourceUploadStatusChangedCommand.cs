using MediatR;
using Querify.Models.QnA.Events;

namespace Querify.QnA.Portal.Business.Source.Commands.NotifySourceUploadStatusChanged;

public sealed class NotifySourceUploadStatusChangedCommand : IRequest
{
    public required SourceUploadStatusChangedIntegrationEvent IntegrationEvent { get; init; }
}
