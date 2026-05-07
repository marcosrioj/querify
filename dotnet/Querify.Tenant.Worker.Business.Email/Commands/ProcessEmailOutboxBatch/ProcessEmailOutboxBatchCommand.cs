using MediatR;

namespace Querify.Tenant.Worker.Business.Email.Commands.ProcessEmailOutboxBatch;

public sealed class ProcessEmailOutboxBatchCommand : IRequest<bool>;
