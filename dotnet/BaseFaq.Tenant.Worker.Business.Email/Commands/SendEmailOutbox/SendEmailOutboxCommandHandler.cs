using MediatR;
using Microsoft.Extensions.Logging;

namespace BaseFaq.Tenant.Worker.Business.Email.Commands.SendEmailOutbox;

public sealed class SendEmailOutboxCommandHandler(
    ILogger<SendEmailOutboxCommandHandler> logger)
    : IRequestHandler<SendEmailOutboxCommand>
{
    public Task Handle(SendEmailOutboxCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workItem = command.WorkItem;

        logger.LogWarning(
            "Email outbox record {EmailOutboxId} to {RecipientEmail} cannot be sent because no email provider is implemented yet.",
            workItem.Id,
            workItem.RecipientEmail);

        throw new InvalidOperationException(
            $"No email provider is implemented yet. Cannot send email to '{workItem.RecipientEmail}'.");
    }
}
