using Querify.QnA.Worker.Business.Source.Abstractions;

namespace Querify.QnA.Worker.Api.Infrastructure;

public sealed class QnAWorkerTenantContext : IQnAWorkerTenantContext
{
    private Guid _tenantId;

    public Guid TenantId => _tenantId;

    public IDisposable UseTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("QnA worker tenant id is required.");
        }

        var previous = _tenantId;
        _tenantId = tenantId;
        return new ResetOnDispose(() => _tenantId = previous);
    }

    private sealed class ResetOnDispose(Action reset) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            reset();
            _disposed = true;
        }
    }
}
