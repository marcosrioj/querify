namespace Querify.QnA.Worker.Business.Source.Abstractions;

public interface IQnAWorkerTenantContext
{
    Guid TenantId { get; }
    IDisposable UseTenant(Guid tenantId);
}
