using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.QnA.Worker.Business.Source.Abstractions;
using System.Net;

namespace Querify.QnA.Worker.Api.Infrastructure;

public sealed class QnAWorkerSessionService(IQnAWorkerTenantContext tenantContext) : ISessionService
{
    public Guid GetTenantId(ModuleEnum module)
    {
        if (module is not ModuleEnum.QnA)
        {
            throw new ApiErrorException(
                $"QnA worker cannot resolve tenant context for module '{module}'.",
                (int)HttpStatusCode.InternalServerError);
        }

        if (tenantContext.TenantId == Guid.Empty)
        {
            throw new ApiErrorException(
                "QnA worker tenant context is not set.",
                (int)HttpStatusCode.InternalServerError);
        }

        return tenantContext.TenantId;
    }

    public Guid GetUserId() => Guid.Empty;

    public string? GetUserName() => "system:qna-worker";
}
