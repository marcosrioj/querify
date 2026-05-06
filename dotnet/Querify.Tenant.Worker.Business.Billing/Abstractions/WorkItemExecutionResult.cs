namespace Querify.Tenant.Worker.Business.Billing.Abstractions;

public readonly record struct WorkItemExecutionResult(
    bool IsSuccess,
    bool ShouldRetry,
    string? FailureReason,
    TimeSpan? RetryAfter)
{
    public static WorkItemExecutionResult Success() => new(true, false, null, null);

    public static WorkItemExecutionResult Retry(string reason, TimeSpan? retryAfter = null) =>
        new(false, true, reason, retryAfter);

    public static WorkItemExecutionResult Fail(string reason) =>
        new(false, false, reason, null);
}
