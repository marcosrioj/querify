namespace Querify.Common.Infrastructure.MassTransit.Models;

public class RabbitMqOption
{
    public const string Name = "RabbitMQ";

    public required string Hostname { get; set; }
    public required int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required RabbitMqExchangeOption Exchange { get; set; }
    public required string QueueName { get; set; }
    public int PrefetchCount { get; set; } = 1;
    public int ConcurrencyLimit { get; set; } = 1;
    public RabbitMqRetryOption Retry { get; set; } = new();
}

public class RabbitMqExchangeOption
{
    public required string Name { get; set; }
    public required string Type { get; set; }
}

public class RabbitMqRetryOption
{
    public int Count { get; set; } = 3;
    public int IntervalMilliseconds { get; set; } = 500;
}