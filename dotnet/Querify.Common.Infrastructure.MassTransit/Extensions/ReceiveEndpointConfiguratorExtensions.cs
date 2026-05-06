using Querify.Common.Infrastructure.MassTransit.Models;
using MassTransit;

namespace Querify.Common.Infrastructure.MassTransit.Extensions;

public static class ReceiveEndpointConfiguratorExtensions
{
    public static void ConfigureResilience(this IReceiveEndpointConfigurator endpoint, RabbitMqOption rabbitMqOption)
    {
        var retryCount = Math.Max(0, rabbitMqOption.Retry.Count);
        if (retryCount == 0)
        {
            return;
        }

        var retryIntervalMs = Math.Max(1, rabbitMqOption.Retry.IntervalMilliseconds);
        endpoint.UseMessageRetry(retry => retry.Interval(retryCount, TimeSpan.FromMilliseconds(retryIntervalMs)));
    }
}