using MediatR;

namespace Querify.QnA.Worker.Business.Source.Commands.ExpirePendingSourceUploads;

public sealed class ExpirePendingSourceUploadsCommand : IRequest<bool>
{
    public required DateTime NowUtc { get; set; }
}
