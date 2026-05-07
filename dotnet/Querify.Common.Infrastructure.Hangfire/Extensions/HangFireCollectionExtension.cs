using System.Text.RegularExpressions;
using Hangfire;
using Hangfire.Heartbeat;
using Hangfire.JobsLogger;
using Hangfire.PostgreSql;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Querify.Common.Infrastructure.Hangfire.Abstractions;
using Querify.Common.Infrastructure.Hangfire.Options;
using Querify.Common.Infrastructure.Hangfire.Services;

namespace Querify.Common.Infrastructure.Hangfire.Extensions;

public static class HangFireCollectionExtension
{
    private static void LoadHangFireOptions(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<HangFireOptions>()
            .Bind(configuration.GetSection(HangFireOptions.Name))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    public static void AddHangFire(this IServiceCollection services,
        IConfiguration configuration, string[]? queues = null)
    {
        LoadHangFireOptions(services, configuration);

        var options = configuration.GetRequiredSection(HangFireOptions.Name).Get<HangFireOptions>();

        if (options is null)
            throw new Exception("Hangfire options not found");

        options.WorkerCount = 20;

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(options.ConnectionString))
            .UseHeartbeatPage(TimeSpan.FromSeconds(30))
            .UseJobsLogger()
        );

        var defaultQueues = new[] { "default" };

        if (queues == null)
        {
            queues = defaultQueues.ToArray(); //copy array
        }
        else
        {
            if (!defaultQueues.All(w => queues.Contains(w)))
            {
                queues = queues.Concat(defaultQueues).ToArray();
            }
        }

        ValidateQueueNames(queues);

        services.AddHangfireServer(o =>
        {
            o.ServerName = "default";
            o.Queues = queues;
            o.WorkerCount = options.WorkerCount;
        });

        services.AddSingleton<IHangFireJobService, HangFireJobService>();
    }

    public static void AddHangFireInMemory(this IServiceCollection services,
        IConfiguration configuration, string[]? queues = null)
    {
        LoadHangFireOptions(services, configuration);

        var options = configuration.GetRequiredSection(HangFireOptions.Name).Get<HangFireOptions>();

        if (options is null)
            throw new Exception("Hangfire options not found");

        options.WorkerCount = 10;

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseInMemoryStorage()
            .UseHeartbeatPage(TimeSpan.FromSeconds(30))
            .UseJobsLogger()
        );

        var defaultQueues = new[] { "default" };

        if (queues == null)
        {
            queues = defaultQueues.ToArray(); //copy array
        }
        else
        {
            if (!defaultQueues.All(w => queues.Contains(w)))
            {
                queues = queues.Concat(defaultQueues).ToArray();
            }
        }

        ValidateQueueNames(queues);


        services.AddHangfireServer(o =>
        {
            o.ServerName = "default";
            o.Queues = queues;
            o.WorkerCount = options.WorkerCount;
        });

        services.AddSingleton<IHangFireJobService, HangFireJobService>();
    }

    public static void UseHangFireDashboard(this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var options = configuration.GetRequiredSection(HangFireOptions.Name).Get<HangFireOptions>();

        if (options?.Dashboard is null)
            throw new Exception("Hangfire Dashboard options not found");

        Console.WriteLine(options.ConnectionString);

        if (!options.Dashboard.Enabled)
        {
            return;
        }

        var opt = new DashboardOptions
        {
            Authorization = new[]
            {
                new HangfireCustomBasicAuthenticationFilter
                {
                    User = options.Dashboard.UserName, Pass = options.Dashboard.Password
                }
            }
        };

        app.UseHangfireDashboard("/HangfireDashboard", opt);
    }

    private static void ValidateQueueNames(string[] array)
    {
        //The Queue name argument must consist of lowercase letters, digits, underscore, and dash (since 1.7.6) characters only.
        string pattern = @"^[a-z0-9_-]+$";
        Regex regex = new Regex(pattern);

        foreach (string str in array)
        {
            if (!regex.IsMatch(str))
            {
                throw new Exception($"Invalid hangfire queue name:{str}");
            }
        }

        return;
    }
}
