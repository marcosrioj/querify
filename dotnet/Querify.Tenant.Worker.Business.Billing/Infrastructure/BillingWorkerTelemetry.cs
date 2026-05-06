using System.Diagnostics;

namespace Querify.Tenant.Worker.Business.Billing.Infrastructure;

public static class BillingWorkerTelemetry
{
    public const string ActivitySourceName = "Querify.Tenant.Worker.Billing";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
