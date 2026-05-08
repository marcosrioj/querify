using Querify.Common.EntityFramework.Tenant.Extensions;
using Querify.Common.Infrastructure.ApiErrorHandling.Extensions;
using Querify.Common.Infrastructure.Core.Extensions;
using Querify.Common.Infrastructure.MediatR.Extensions;
using Querify.Common.Infrastructure.Mvc.Filters;
using Querify.Common.Infrastructure.Sentry.Extensions;
using Querify.Common.Infrastructure.Signalr.Portal.Hubs;
using Querify.Common.Infrastructure.Signalr.Portal.Options;
using Querify.Common.Infrastructure.Swagger.Extensions;
using Querify.Common.Infrastructure.Telemetry.Extensions;
using Querify.Models.Common.Enums;
using Querify.QnA.Portal.Api.Extensions;
using Querify.QnA.Portal.Business.Source.Infrastructure;

namespace Querify.QnA.Portal.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var portalSignalROptions = PortalSignalROptions.FromConfiguration(builder.Configuration, ModuleEnum.QnA);

        builder.Host.UseDefaultServiceProvider(opt =>
        {
            opt.ValidateOnBuild = true;
            opt.ValidateScopes = true;
        });

        builder.Services.AddOpenApi();
        builder.Services.AddCustomCors(builder.Configuration);
        builder.Services.AddSwaggerWithAuth(builder.Configuration);
        builder.Services.AddDefaultAuthentication(builder.Configuration, portalSignalROptions.NotificationsHubPath);
        builder.Services.AddTenantDb(builder.Configuration.GetConnectionString("TenantDb"));
        builder.Services.AddSessionService(builder.Configuration);
        builder.Services.AddLogging(c =>
        {
            c.SetMinimumLevel(LogLevel.Information);
            c.AddConsole();
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddTelemetry(
            builder.Configuration,
            builder.Environment,
            SourcePortalTelemetry.ActivitySourceName);
        builder.Services.AddFeatures(builder.Configuration);
        builder.Services.AddMediatRLogging();
        builder.WebHost.AddConfiguredSentry(builder.Environment);
        builder.Services.AddControllers(options => { options.Filters.Add(new StringTrimmingActionFilter()); })
            .AddJsonOptions(options => { });

        var app = builder.Build();

        app.UseApiErrorHandlingMiddleware();
        app.UseRouting();

        if (!app.Environment.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUIWithAuth();
            app.MapOpenApi();
        }

        app.UseCustomCors(builder.Configuration);
        app.UseConfiguredSentry();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseTenantResolution(ModuleEnum.QnA);
        app.MapHub<PortalNotificationsHub>(portalSignalROptions.NotificationsHubPath).RequireAuthorization();
        app.MapControllers().RequireAuthorization();
        app.Run();
    }
}
