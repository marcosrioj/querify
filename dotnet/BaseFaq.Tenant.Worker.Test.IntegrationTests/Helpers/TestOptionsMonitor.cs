using Microsoft.Extensions.Options;

namespace BaseFaq.Tenant.Worker.Test.IntegrationTests.Helpers;

/// <summary>
/// Minimal IOptionsMonitor stub that always returns the provided value.
/// </summary>
public sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
{
    public TestOptionsMonitor(T value)
    {
        CurrentValue = value;
    }

    public T CurrentValue { get; set; }

    public T Get(string? name) => CurrentValue;

    public IDisposable? OnChange(Action<T, string?> listener) => null;
}
