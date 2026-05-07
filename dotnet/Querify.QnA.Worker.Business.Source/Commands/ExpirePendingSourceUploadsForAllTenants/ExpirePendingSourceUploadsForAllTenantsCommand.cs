using MediatR;

namespace Querify.QnA.Worker.Business.Source.Commands.ExpirePendingSourceUploadsForAllTenants;

public sealed class ExpirePendingSourceUploadsForAllTenantsCommand : IRequest<bool>
{
    public required DateTime NowUtc { get; set; }
}
