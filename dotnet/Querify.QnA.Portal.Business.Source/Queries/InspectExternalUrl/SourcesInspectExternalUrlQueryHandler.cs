using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.QnA.Dtos.Source;
using MediatR;

namespace Querify.QnA.Portal.Business.Source.Queries.InspectExternalUrl;

public sealed class SourcesInspectExternalUrlQueryHandler(SourceExternalUrlInspectionCoordinator coordinator)
    : IRequestHandler<SourcesInspectExternalUrlQuery, SourceExternalUrlInspectionDto>
{
    public Task<SourceExternalUrlInspectionDto> Handle(
        SourcesInspectExternalUrlQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        if (!Uri.TryCreate(request.Request.Locator, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ApiErrorException("External URL must use HTTP or HTTPS.", (int)HttpStatusCode.UnprocessableEntity);

        return coordinator.InspectAsync(uri, cancellationToken);
    }
}
