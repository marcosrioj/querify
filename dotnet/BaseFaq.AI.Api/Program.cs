using BaseFaq.AI.Api.Extensions;
using BaseFaq.AI.Api.Infrastructure;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Common.Infrasctructure.Telemetry.Extensions;

namespace BaseFaq.AI.Api;

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

        builder.Services.AddLogging(c =>
        {
            c.SetMinimumLevel(LogLevel.Information);
            c.AddConsole();
        });

        builder.Services.AddScoped<ISessionService, AiWorkerSessionService>();

        builder.Services.AddTelemetry(
            builder.Configuration,
            builder.Environment);

        builder.Services.AddFeatures(builder.Configuration);

        var app = builder.Build();

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

        app.Run();
    }
}
