using Querify.Models.Common.Enums;

namespace Querify.Common.Infrastructure.ApiErrorHandling.Exception;

public class ApiErrorConfirmationException(
    string message = "Api error",
    int errorCode = 241) : System.Exception(message)
{
    public int ErrorCode { get; private set; } = errorCode;
}