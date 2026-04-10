using System.Diagnostics;

namespace BaseFaq.Tenant.Worker.Business.Billing.Infrastructure;

public static class BillingWorkerTelemetry
{
    public const string ActivitySourceName = "BaseFaq.Tenant.Worker.Billing";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
