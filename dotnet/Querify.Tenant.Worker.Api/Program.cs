using Querify.Common.EntityFramework.Tenant.Extensions;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Telemetry.Extensions;
using Querify.Tenant.Worker.Api.Extensions;
using Querify.Tenant.Worker.Api.Infrastructure;
using Querify.Tenant.Worker.Business.Billing.Infrastructure;
using Querify.Tenant.Worker.Business.Email.Infrastructure;

namespace Querify.Tenant.Worker.Api;

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
