using Querify.QnA.Worker.Business.Source.Abstractions;

namespace Querify.QnA.Worker.Test.IntegrationTests.Helpers;

public sealed class TestTenantContext : IQnAWorkerTenantContext
{
    public Guid TenantId { get; private set; }

    public IDisposable UseTenant(Guid tenantId)
    {
        var previous = TenantId;
        TenantId = tenantId;
        return new ResetOnDispose(() => TenantId = previous);
    }

    private sealed class ResetOnDispose(Action reset) : IDisposable
    {
        public void Dispose() => reset();
    }
}
