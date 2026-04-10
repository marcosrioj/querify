using System.Diagnostics;

namespace BaseFaq.Tenant.Worker.Business.Email.Infrastructure;

public static class EmailWorkerTelemetry
{
    public const string ActivitySourceName = "BaseFaq.Tenant.Worker.Email";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
