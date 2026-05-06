namespace Querify.Tenant.Worker.Business.Email.Abstractions;

public interface IEmailOutboxProcessor
{
    Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default);
}
