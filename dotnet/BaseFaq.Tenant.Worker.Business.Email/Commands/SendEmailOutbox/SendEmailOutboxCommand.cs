using BaseFaq.Common.EntityFramework.Tenant.Entities;
using MediatR;

namespace BaseFaq.Tenant.Worker.Business.Email.Commands.SendEmailOutbox;

public sealed record SendEmailOutboxCommand(
    EmailOutbox WorkItem) : IRequest;
