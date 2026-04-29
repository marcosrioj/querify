using BaseFaq.Common.EntityFramework.Tenant.Extensions;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Extensions;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Extensions;
using BaseFaq.Common.Infrastructure.MediatR.Extensions;
using BaseFaq.Common.Infrastructure.Mvc.Filters;
using BaseFaq.Common.Infrastructure.Sentry.Extensions;
using BaseFaq.Common.Infrastructure.Swagger.Extensions;
using BaseFaq.Tenant.Public.Api.Extensions;
using BaseFaq.Tenant.Public.Api.Infrastructure;

namespace BaseFaq.Tenant.Public.Api;

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
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ISessionService, TenantPublicSessionService>();
        builder.Services.AddTenantDb(builder.Configuration.GetConnectionString("TenantDb"));

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
        app.MapControllers();

        app.Run();
    }
}
