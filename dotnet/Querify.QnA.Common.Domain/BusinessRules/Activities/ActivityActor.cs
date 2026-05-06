using Querify.Models.QnA.Enums;

namespace Querify.QnA.Common.Domain.BusinessRules.Activities;

public sealed record ActivityActor(
    ActorKind ActorKind,
    string UserPrint,
    string Ip,
    string UserAgent,
    Guid? UserId,
    string? UserName,
    bool IsPublic)
{
    public string AuditUserId => UserId?.ToString("D") ?? UserPrint;

    public string DisplayName => IsPublic
        ? UserPrint
        : string.IsNullOrWhiteSpace(UserName)
            ? AuditUserId
            : UserName;

    public string ActorLabel => DisplayName;
}
