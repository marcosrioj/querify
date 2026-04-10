using BaseFaq.Common.EntityFramework.Tenant.Extensions;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Telemetry.Extensions;
using BaseFaq.Tenant.Worker.Api.Extensions;
using BaseFaq.Tenant.Worker.Api.Infrastructure;
using BaseFaq.Tenant.Worker.Business.Billing.Infrastructure;
using BaseFaq.Tenant.Worker.Business.Email.Infrastructure;

namespace BaseFaq.Tenant.Worker.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseDefaultServiceProvider(opt =>
            {
                opt.ValidateOnBuild = true;
                opt.ValidateScopes = true;
            })
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddHttpContextAccessor();
                services.AddScoped<ISessionService, TenantWorkerSessionService>();
                services.AddTenantDb(context.Configuration.GetConnectionString("TenantDb"));
                services.AddTelemetry(
                    context.Configuration,
                    context.HostingEnvironment,
                    BillingWorkerTelemetry.ActivitySourceName,
                    EmailWorkerTelemetry.ActivitySourceName);
                services.AddTenantWorkerFeatures(context.Configuration);
            })
            .Build();

        host.Run();
    }
}
