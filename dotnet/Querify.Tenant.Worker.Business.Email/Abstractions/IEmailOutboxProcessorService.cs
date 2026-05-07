namespace Querify.Tenant.Worker.Business.Email.Abstractions;

public interface IEmailOutboxProcessorService
{
    Task<bool> ProcessBatchAsync(CancellationToken cancellationToken = default);
}
