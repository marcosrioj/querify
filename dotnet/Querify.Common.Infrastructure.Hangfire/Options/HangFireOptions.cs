namespace Querify.Common.Infrastructure.Hangfire.Options;

public class HangFireOptions
{
    public const string Name = "HangFire";

    public HangFireDashboardOptions Dashboard { get; set; } = default!;

    public string? ConnectionString { get; set; }

    public int WorkerCount { get; set; } = 2;

    public string SchemaName { get; set; } = "hangfire";

    public bool PrepareSchemaIfNecessary { get; set; } = true;

    public int StartupConnectionMaxRetries { get; set; } = 5;

    public int StartupConnectionBaseDelaySeconds { get; set; } = 1;

    public int StartupConnectionMaxDelaySeconds { get; set; } = 60;

    public bool AllowDegradedModeWithoutStorage { get; set; } = true;
}

public class HangFireDashboardOptions
{
    public bool Enabled { get; set; }

    public string UserName { get; set; } = default!;

    public string Password { get; set; } = default!;
}
