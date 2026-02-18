using BaseFaq.Common.Infrastructure.MediatR.Logging;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BaseFaq.AI.Test.IntegrationTest.Tests.Infrastructure;

public sealed class LoggingRedactionTests
{
    [Fact]
    public async Task LoggingBehavior_RedactsSensitiveValues_FromRequestAndResponsePayloads()
    {
        const string apiKey = "sk-live-sensitive-api-key";
        const string clientKey = "client-key-sensitive-value";
        const string password = "super-sensitive-password";
        const string authHeader = "Bearer sensitive-auth-token";
        const string responseToken = "sensitive-response-token";
        const string safeValue = "safe-value";

        var logger = new TestLogger<LoggingBehavior<SensitiveRequest, SensitiveResponse>>();
        var behavior = new LoggingBehavior<SensitiveRequest, SensitiveResponse>(logger);
        var request = new SensitiveRequest
        {
            ApiKey = apiKey,
            ClientKey = clientKey,
            Profile = new SensitiveProfile
            {
                Password = password,
                Headers = new Dictionary<string, string>
                {
                    ["Authorization"] = authHeader
                }
            }
        };
        var response = new SensitiveResponse
        {
            AccessToken = responseToken,
            SafeEcho = safeValue
        };

        await behavior.Handle(request, _ => Task.FromResult(response), CancellationToken.None);

        var logs = string.Join(Environment.NewLine, logger.Messages);

        Assert.DoesNotContain(apiKey, logs, StringComparison.Ordinal);
        Assert.DoesNotContain(clientKey, logs, StringComparison.Ordinal);
        Assert.DoesNotContain(password, logs, StringComparison.Ordinal);
        Assert.DoesNotContain(authHeader, logs, StringComparison.Ordinal);
        Assert.DoesNotContain(responseToken, logs, StringComparison.Ordinal);
        Assert.Contains("[REDACTED]", logs, StringComparison.Ordinal);
        Assert.Contains(safeValue, logs, StringComparison.Ordinal);
    }

    private sealed class SensitiveRequest
    {
        public string ApiKey { get; init; } = string.Empty;
        public string ClientKey { get; init; } = string.Empty;
        public SensitiveProfile Profile { get; init; } = new();
    }

    private sealed class SensitiveProfile
    {
        public string Password { get; init; } = string.Empty;
        public Dictionary<string, string> Headers { get; init; } = [];
    }

    private sealed class SensitiveResponse
    {
        public string AccessToken { get; init; } = string.Empty;
        public string SafeEcho { get; init; } = string.Empty;
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        IDisposable? ILogger.BeginScope<TState>(TState state)
        {
            return null;
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}