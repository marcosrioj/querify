namespace Querify.Common.Infrastructure.Hangfire.Options;

public class HangFireOptions
{
    public const string Name = "HangFire";

    public HangFireDashboardOptions Dashboard { get; set; } = default!;

    public string ConnectionString { get; set; } = default!;

    public int WorkerCount { get; set; } = 2;
}

public class HangFireDashboardOptions
{
    public bool Enabled { get; set; }

    public string UserName { get; set; } = default!;

    public string Password { get; set; } = default!;
}