using System.Text.Json;

namespace Querify.Common.Infrastructure.ApiErrorHandling.Exception;

public class ApiErrorException(
    string message = "Api error",
    int errorCode = 240,
    object? data = null) : System.Exception(message)
{
    public int ErrorCode { get; } = errorCode;
    private new object? Data { get; } = data;

    public ApiErrorResult GetError()
    {
        var dataResult = Data != null ? JsonSerializer.Serialize(Data) : null;

        return new ApiErrorResult
        {
            ErrorCode = ErrorCode,
            MessageError = Message,
            Data = dataResult
        };
    }
}

public class ApiErrorResult
{
    public int ErrorCode { get; set; }
    public string MessageError { get; set; } = string.Empty;
    public object? Data { get; set; }
}