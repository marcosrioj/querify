using MediatR;

namespace Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSourcesForAllTenants;

public sealed class VerifyUploadedSourcesForAllTenantsCommand : IRequest<bool>
{
    public int BatchSize { get; set; } = 20;
}
