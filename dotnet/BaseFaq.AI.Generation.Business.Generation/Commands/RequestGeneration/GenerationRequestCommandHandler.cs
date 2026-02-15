using BaseFaq.AI.Common.Contracts.Generation;
using BaseFaq.Models.Ai.Dtos.Generation;
using MassTransit;
using MediatR;

namespace BaseFaq.AI.Generation.Business.Generation.Commands.RequestGeneration;

public sealed class GenerationRequestCommandHandler(IPublishEndpoint publishEndpoint)
    : IRequestHandler<GenerationRequestCommand, Guid>
{
    private const int MaxLanguageLength = 16;
    private const int MaxIdempotencyKeyLength = 128;

    public async Task<Guid> Handle(
        GenerationRequestCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Request);

        var normalizedIdempotencyKey = ValidateRequest(command.Request, command.IdempotencyKey);

        var requestedUtc = DateTime.UtcNow;
        var correlationId = Guid.NewGuid();

        var message = new FaqGenerationRequestedV1
        {
            CorrelationId = correlationId,
            FaqId = command.Request.FaqId,
            TenantId = command.Request.TenantId,
            RequestedByUserId = command.Request.RequestedByUserId ?? Guid.Empty,
            Language = command.Request.Language,
            IdempotencyKey = normalizedIdempotencyKey,
            RequestedUtc = requestedUtc
        };

        await publishEndpoint.Publish(message, cancellationToken);

        return correlationId;
    }

    private static string ValidateRequest(GenerationRequestDto request, string? idempotencyKey)
    {
        if (request.FaqId == Guid.Empty)
        {
            throw new ArgumentException("FaqId is required.", nameof(request));
        }

        if (request.TenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Language) || request.Language.Length > MaxLanguageLength)
        {
            throw new ArgumentException(
                $"Language is required and must have at most {MaxLanguageLength} characters.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency-Key header is required.", nameof(idempotencyKey));
        }

        var normalizedIdempotencyKey = idempotencyKey.Trim();
        if (normalizedIdempotencyKey.Length > MaxIdempotencyKeyLength)
        {
            throw new ArgumentException(
                $"IdempotencyKey must have at most {MaxIdempotencyKeyLength} characters.",
                nameof(idempotencyKey));
        }

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey) &&
            !string.Equals(request.IdempotencyKey, normalizedIdempotencyKey, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Request body idempotency key does not match Idempotency-Key header.",
                nameof(request));
        }

        return normalizedIdempotencyKey;
    }
}