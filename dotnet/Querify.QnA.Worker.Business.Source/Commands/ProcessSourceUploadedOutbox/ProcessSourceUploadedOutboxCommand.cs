using MediatR;

namespace Querify.QnA.Worker.Business.Source.Commands.ProcessSourceUploadedOutbox;

public sealed class ProcessSourceUploadedOutboxCommand : IRequest<bool>;
