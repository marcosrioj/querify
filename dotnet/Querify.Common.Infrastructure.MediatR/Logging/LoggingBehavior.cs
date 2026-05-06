using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Querify.Common.Infrastructure.MediatR.Logging;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private const string RedactedValue = "[REDACTED]";

    private static readonly string[] SensitiveNameParts =
    [
        "apikey",
        "api_key",
        "token",
        "secret",
        "password",
        "clientkey",
        "client_key",
        "authorization",
        "connectionstring",
        "connection_string"
    ];

    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }


    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = request.GetType().Name;
        var requestGuid = Guid.NewGuid().ToString();

        var requestNameWithGuid = $"{requestName} [{requestGuid}]";

        _logger.LogInformation("===================================================================================");
        _logger.LogInformation("[START] Handling {RequestNameWithGuid}", requestNameWithGuid);

        var stopwatch = Stopwatch.StartNew();
        TResponse response;

        try
        {
            try
            {
                _logger.LogDebug("===================================================================================");
                _logger.LogDebug("Handling {RequestNameWithGuid} - Parameters: {RequestPayload}",
                    requestNameWithGuid,
                    SerializeWithRedaction(request));
                _logger.LogDebug("===================================================================================");
            }
            catch (NotSupportedException)
            {
                _logger.LogWarning("[Serialization ERROR] {RequestNameWithGuid} Could not serialize the request.",
                    requestNameWithGuid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[Serialization ERROR] {RequestNameWithGuid} Could not serialize the request.",
                    requestNameWithGuid);
            }


            response = await next(); // Execution moves to the handler

            // Log the response details
            try
            {
                _logger.LogDebug("===================================================================================");
                _logger.LogDebug("Handled Response: {ResponsePayload}", SerializeWithRedaction(response));
                _logger.LogDebug("===================================================================================");
            }
            catch (NotSupportedException)
            {
                _logger.LogWarning("[Serialization ERROR] {RequestNameWithGuid} Could not serialize the response.",
                    requestNameWithGuid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[Serialization ERROR] {RequestNameWithGuid} Could not serialize the response.",
                    requestNameWithGuid);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("===================================================================================");
            _logger.LogError(ex, "Error handling {RequestNameWithGuid}", requestNameWithGuid);
            _logger.LogError("===================================================================================");


            throw; // Propagate the exception
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("[END] Handled {RequestNameWithGuid} in {ElapsedMilliseconds} ms",
                requestNameWithGuid,
                stopwatch.Elapsed.TotalMilliseconds);
            _logger.LogInformation(
                "===================================================================================");
        }

        return response;
    }

    private static string SerializeWithRedaction<T>(T value)
    {
        var jsonNode = JsonSerializer.SerializeToNode(value, JsonSerializerOptions);
        RedactNode(jsonNode);
        return jsonNode?.ToJsonString(JsonSerializerOptions) ?? "null";
    }

    private static void RedactNode(JsonNode? node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToArray())
            {
                if (IsSensitiveProperty(property.Key))
                {
                    jsonObject[property.Key] = RedactedValue;
                    continue;
                }

                RedactNode(property.Value);
            }

            return;
        }

        if (node is JsonArray jsonArray)
        {
            foreach (var child in jsonArray)
            {
                RedactNode(child);
            }
        }
    }

    private static bool IsSensitiveProperty(string propertyName)
    {
        var normalized = propertyName.Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        return SensitiveNameParts.Any(part =>
            normalized.Contains(part.Replace("_", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal));
    }
}