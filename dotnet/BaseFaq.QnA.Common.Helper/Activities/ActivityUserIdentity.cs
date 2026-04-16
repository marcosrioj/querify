namespace BaseFaq.QnA.Common.Helper.Activities;

public readonly record struct ActivityUserIdentity(
    string UserPrint,
    string Ip,
    string UserAgent,
    Guid? AuthenticatedUserId)
{
    public bool IsAuthenticated => AuthenticatedUserId.HasValue;
}
