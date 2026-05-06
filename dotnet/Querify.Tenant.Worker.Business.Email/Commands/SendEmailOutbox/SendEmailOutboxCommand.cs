using Querify.Common.EntityFramework.Tenant.Entities;
using MediatR;

namespace Querify.Tenant.Worker.Business.Email.Commands.SendEmailOutbox;

public sealed record SendEmailOutboxCommand(
    EmailOutbox WorkItem) : IRequest;
