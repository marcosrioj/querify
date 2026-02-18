using BaseFaq.Common.Infrastructure.MassTransit.Extensions;
using BaseFaq.Common.Infrastructure.MassTransit.Models;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BaseFaq.AI.Test.IntegrationTest.Tests.Infrastructure;

public class RetryAndDlqPolicyTests
{
    [Fact]
    public async Task ConfigureResilience_RetriesAndPublishesFaultWhenRetriesAreExhausted()
    {
        AlwaysFailConsumer.Attempts = 0;

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<AlwaysFailConsumer>();

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ReceiveEndpoint("retry-dlq-validation", endpoint =>
                    {
                        endpoint.ConfigureResilience(CreateRabbitMqOption(retryCount: 2, intervalMs: 10));
                        endpoint.ConfigureConsumer<AlwaysFailConsumer>(context);
                    });
                });
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            await harness.Bus.Publish(new RetryValidationMessage { Id = Guid.NewGuid() });

            Assert.True(await harness.Published.Any<Fault<RetryValidationMessage>>());
            Assert.Equal(3, AlwaysFailConsumer.Attempts);
        }
        finally
        {
            await harness.Stop();
        }
    }

    private static RabbitMqOption CreateRabbitMqOption(int retryCount, int intervalMs)
    {
        return new RabbitMqOption
        {
            Hostname = "localhost",
            Port = 5672,
            Username = "guest",
            Password = "guest",
            QueueName = "test",
            PrefetchCount = 1,
            ConcurrencyLimit = 1,
            Retry = new RabbitMqRetryOption
            {
                Count = retryCount,
                IntervalMilliseconds = intervalMs
            },
            Exchange = new RabbitMqExchangeOption
            {
                Name = "test",
                Type = "fanout"
            }
        };
    }

    private sealed class AlwaysFailConsumer : IConsumer<RetryValidationMessage>
    {
        public static int Attempts { get; set; }

        public Task Consume(ConsumeContext<RetryValidationMessage> context)
        {
            Attempts++;
            throw new InvalidOperationException("forced failure");
        }
    }

    private sealed class RetryValidationMessage
    {
        public Guid Id { get; set; }
    }
}