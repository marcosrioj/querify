using Hangfire;
using Microsoft.Extensions.Options;
using Querify.QnA.Worker.Business.Source.BackgroundServices;
using Querify.QnA.Worker.Business.Source.Options;

namespace Querify.QnA.Worker.Api.HostedServices;

public sealed class SourceUploadHangfireJobRegistrationHostedService(
    IRecurringJobManager recurringJobManager,
    IOptionsMonitor<SourceUploadVerificationSweepOptions> optionsMonitor,
    ILogger<SourceUploadHangfireJobRegistrationHostedService> logger)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var options = optionsMonitor.CurrentValue;
        if (!options.Enabled)
        {
            recurringJobManager.RemoveIfExists(options.RecurringJobId);
            logger.LogInformation(
                "Source upload verification Hangfire job {RecurringJobId} is disabled and was removed if present.",
                options.RecurringJobId);
            return Task.CompletedTask;
        }

        recurringJobManager.AddOrUpdate<SourceUploadVerificationBackgroundService>(
            options.RecurringJobId,
            options.QueueName,
            service => service.VerifyUploadedSourcesAsync(CancellationToken.None),
            options.CronExpression,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        logger.LogInformation(
            "Registered source upload verification Hangfire job {RecurringJobId} on queue {QueueName} with cron {CronExpression}.",
            options.RecurringJobId,
            options.QueueName,
            options.CronExpression);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
