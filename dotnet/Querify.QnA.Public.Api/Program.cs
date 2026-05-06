using Querify.Common.EntityFramework.Tenant.Extensions;
using Querify.Common.Infrastructure.ApiErrorHandling.Extensions;
using Querify.Common.Infrastructure.Core.Extensions;
using Querify.Common.Infrastructure.MediatR.Extensions;
using Querify.Common.Infrastructure.Mvc.Filters;
using Querify.Common.Infrastructure.Sentry.Extensions;
using Querify.Common.Infrastructure.Swagger.Extensions;
using Querify.QnA.Public.Api.Extensions;

namespace Querify.QnA.Public.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseDefaultServiceProvider(opt =>
        {
            opt.ValidateOnBuild = true;
            opt.ValidateScopes = true;
        });

        builder.Services.AddOpenApi();
        builder.Services.AddCustomCors(builder.Configuration);
        builder.Services.AddSwagger(builder.Configuration);
        builder.Services.AddTenantDb(builder.Configuration.GetConnectionString("TenantDb"));
        builder.Services.AddSessionService(builder.Configuration);
        builder.Services.AddLogging(c =>
        {
            c.SetMinimumLevel(LogLevel.Information);
            c.AddConsole();
        });
        builder.Services.AddEndpointsApiExplorer();
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
            app.UseSwaggerUI();
            app.MapOpenApi();
        }

        app.UseCustomCors(builder.Configuration);
        app.UseConfiguredSentry();
        app.UseClientKeyResolution();
        app.MapControllers();
        app.Run();
    }
}
