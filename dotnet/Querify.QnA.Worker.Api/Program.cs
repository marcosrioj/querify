using Microsoft.AspNetCore.Hosting;
using Querify.Common.EntityFramework.Tenant.Extensions;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Common.Infrastructure.Hangfire.Extensions;
using Querify.Common.Infrastructure.Storage.Extensions;
using Querify.Common.Infrastructure.Telemetry.Extensions;
using Querify.QnA.Common.Persistence.QnADb.Extensions;
using Querify.QnA.Worker.Api.Extensions;
using Querify.QnA.Worker.Api.Infrastructure;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Infrastructure;
using Querify.QnA.Worker.Business.Source.Options;

namespace Querify.QnA.Worker.Api;

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
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.Configure((context, app) =>
                {
                    app.UseHangFireDashboard(context.Configuration);
                });
            })
            .ConfigureServices((context, services) =>
            {
                services.AddHttpContextAccessor();
                services.AddScoped<IQnAWorkerTenantContext, QnAWorkerTenantContext>();
                services.AddScoped<ISessionService, QnAWorkerSessionService>();
                services.AddTenantDb(context.Configuration.GetConnectionString("TenantDb"));
                services.AddQnADb();
                services.AddObjectStorage(context.Configuration);
                var sourceUploadVerificationOptions = context.Configuration
                    .GetSection(SourceUploadVerificationSweepOptions.SectionName)
                    .Get<SourceUploadVerificationSweepOptions>() ?? new SourceUploadVerificationSweepOptions();
                services.AddHangFire(context.Configuration, [sourceUploadVerificationOptions.QueueName]);
                services.AddTelemetry(
                    context.Configuration,
                    context.HostingEnvironment,
                    SourceWorkerTelemetry.ActivitySourceName);
                services.AddQnAWorkerFeatures(context.Configuration, context.HostingEnvironment);
            })
            .Build();

        host.Run();
    }
}
