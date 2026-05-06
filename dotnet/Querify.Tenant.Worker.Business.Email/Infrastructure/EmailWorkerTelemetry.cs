using System.Diagnostics;

namespace Querify.Tenant.Worker.Business.Email.Infrastructure;

public static class EmailWorkerTelemetry
{
    public const string ActivitySourceName = "Querify.Tenant.Worker.Email";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
