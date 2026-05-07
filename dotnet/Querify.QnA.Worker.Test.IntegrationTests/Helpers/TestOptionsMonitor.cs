using Microsoft.Extensions.Options;

namespace Querify.QnA.Worker.Test.IntegrationTests.Helpers;

public sealed class TestOptionsMonitor<T>(T value) : IOptionsMonitor<T>
{
    public T CurrentValue => value;

    public T Get(string? name) => value;

    public IDisposable? OnChange(Action<T, string?> listener) => null;
}
