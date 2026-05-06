namespace Querify.QnA.Common.Domain.BusinessRules.Activities;

public readonly record struct ActivityUserIdentity(
    string UserPrint,
    string Ip,
    string UserAgent,
    Guid? AuthenticatedUserId)
{
    public bool IsAuthenticated => AuthenticatedUserId.HasValue;
}